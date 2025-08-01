using System;
using System.Diagnostics;
using System.Threading.Tasks;
using MassTransit;
using Microsoft.Extensions.Logging;
using Validation.Domain.Events;
using Validation.Domain.Validation;
using Validation.Infrastructure.Reliability;
using Validation.Infrastructure.Repositories;

namespace Validation.Infrastructure.Messaging;

public class ReliableDeleteValidationConsumer<T> : IConsumer<DeleteRequested>
{
    private readonly IValidationPlanProvider _planProvider;
    private readonly SummarisationValidator _validator;
    private readonly DeletePipelineReliabilityPolicy _reliabilityPolicy;
    private readonly ISaveAuditRepository _auditRepository;
    private readonly ILogger<ReliableDeleteValidationConsumer<T>> _logger;
    private static readonly ActivitySource ActivitySource = new("Validation.Infrastructure.Messaging");

    public ReliableDeleteValidationConsumer(
        IValidationPlanProvider planProvider,
        SummarisationValidator validator,
        DeletePipelineReliabilityPolicy reliabilityPolicy,
        ISaveAuditRepository auditRepository,
        ILogger<ReliableDeleteValidationConsumer<T>> logger)
    {
        _planProvider = planProvider;
        _validator = validator;
        _reliabilityPolicy = reliabilityPolicy;
        _auditRepository = auditRepository;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<DeleteRequested> context)
    {
        using var activity = ActivitySource.StartActivity("DeleteValidation");
        activity?.SetTag("entity.id", context.Message.Id.ToString());
        activity?.SetTag("entity.type", typeof(T).Name);

        _logger.LogInformation("Starting delete validation for entity {EntityId} of type {EntityType}",
            context.Message.Id, typeof(T).Name);

        try
        {
            await _reliabilityPolicy.ExecuteAsync(async ct =>
            {
                await ValidateDeleteAsync(context, ct);
            }, context.CancellationToken);

            _logger.LogInformation("Delete validation completed successfully for entity {EntityId}",
                context.Message.Id);
        }
        catch (DeletePipelineCircuitOpenException ex)
        {
            _logger.LogError(ex, "Delete validation failed due to circuit breaker being open for entity {EntityId}",
                context.Message.Id);
            
            // Send to dead letter queue or handle graceful degradation
            await context.Publish(new DeleteValidationFailed(context.Message.Id, "Circuit breaker open", typeof(T).Name),
                context.CancellationToken);
            
            throw;
        }
        catch (DeletePipelineReliabilityException ex)
        {
            _logger.LogError(ex, "Delete validation failed after all retry attempts for entity {EntityId}",
                context.Message.Id);
            
            await context.Publish(new DeleteValidationFailed(context.Message.Id, ex.Message, typeof(T).Name),
                context.CancellationToken);
            
            throw;
        }
    }

    private async Task ValidateDeleteAsync(ConsumeContext<DeleteRequested> context, CancellationToken cancellationToken)
    {
        // Get the last audit record to understand the current state
        var lastAudit = await _auditRepository.GetLastAsync(context.Message.Id, cancellationToken);
        
        if (lastAudit == null)
        {
            _logger.LogWarning("No audit record found for entity {EntityId}. Allowing delete.",
                context.Message.Id);
        }

        // Get validation rules for this entity type
        var rules = _planProvider.GetRules<T>();
        
        // Validate deletion (using zero metrics since the entity is being deleted)
        var isValid = _validator.Validate(lastAudit?.Metric ?? 0m, 0m, rules);

        _logger.LogDebug("Delete validation result for entity {EntityId}: {IsValid}",
            context.Message.Id, isValid);

        // Create audit record for the delete operation
        var deleteAudit = new SaveAudit
        {
            Id = Guid.NewGuid(),
            EntityId = context.Message.Id,
            IsValid = isValid,
            Metric = 0m, // Zero metric for delete operation
            Timestamp = DateTime.UtcNow
        };

        await _auditRepository.AddAsync(deleteAudit, cancellationToken);

        // Publish validation result
        if (isValid)
        {
            await context.Publish(new DeleteValidated(context.Message.Id, deleteAudit.Id, typeof(T).Name),
                cancellationToken);
        }
        else
        {
            await context.Publish(new DeleteRejected(context.Message.Id, deleteAudit.Id, "Validation failed", typeof(T).Name),
                cancellationToken);
        }
    }
}

