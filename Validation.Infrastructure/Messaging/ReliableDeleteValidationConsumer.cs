using System.Diagnostics;
using MassTransit;
using Microsoft.Extensions.Logging;
using ValidationFlow.Messages;
using Validation.Domain.Validation;
using Validation.Infrastructure.Reliability;
using Validation.Infrastructure.Repositories;

namespace Validation.Infrastructure.Messaging;

public class ReliableDeleteValidationConsumer<T> : IConsumer<DeleteRequested<T>>
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

    public async Task Consume(ConsumeContext<DeleteRequested<T>> context)
    {
        using var activity = ActivitySource.StartActivity("DeleteValidation");
        activity?.SetTag("entity.id", context.Message.EntityId.ToString());
        activity?.SetTag("entity.type", context.Message.EntityType);

        _logger.LogInformation("Starting delete validation for entity {EntityId} of type {EntityType}",
            context.Message.EntityId, context.Message.EntityType);

        try
        {
            await _reliabilityPolicy.ExecuteAsync(async ct =>
            {
                await ValidateDeleteAsync(context, ct);
            }, context.CancellationToken);

            _logger.LogInformation("Delete validation completed successfully for entity {EntityId}",
                context.Message.EntityId);
        }
        catch (DeletePipelineCircuitOpenException ex)
        {
            _logger.LogError(ex, "Delete validation failed due to circuit breaker being open for entity {EntityId}",
                context.Message.EntityId);

            await context.Publish(new DeleteCommitFault<T>(context.Message.AppName, context.Message.EntityType, context.Message.EntityId, "Circuit breaker open"),
                context.CancellationToken);

            throw;
        }
        catch (DeletePipelineReliabilityException ex)
        {
            _logger.LogError(ex, "Delete validation failed after all retry attempts for entity {EntityId}",
                context.Message.EntityId);

            await context.Publish(new DeleteCommitFault<T>(context.Message.AppName, context.Message.EntityType, context.Message.EntityId, ex.Message),
                context.CancellationToken);

            throw;
        }
    }

    private async Task ValidateDeleteAsync(ConsumeContext<DeleteRequested<T>> context, CancellationToken cancellationToken)
    {
        var lastAudit = await _auditRepository.GetLastAsync(context.Message.EntityId, cancellationToken);

        if (lastAudit == null)
        {
            _logger.LogWarning("No audit record found for entity {EntityId}. Allowing delete.",
                context.Message.EntityId);
        }

        var rules = _planProvider.GetRules<T>();

        var isValid = _validator.Validate(lastAudit?.Metric ?? 0m, 0m, rules);

        _logger.LogDebug("Delete validation result for entity {EntityId}: {IsValid}",
            context.Message.EntityId, isValid);

        var deleteAudit = new SaveAudit
        {
            Id = Guid.NewGuid(),
            EntityId = context.Message.EntityId,
            IsValid = isValid,
            Metric = 0m,
            Timestamp = DateTime.UtcNow
        };

        await _auditRepository.AddAsync(deleteAudit, cancellationToken);

        if (isValid)
        {
            await context.Publish(new DeleteValidated<T>(context.Message.AppName, context.Message.EntityType, context.Message.EntityId, true),
                cancellationToken);
        }
        else
        {
            await context.Publish(new DeleteRejected<T>(context.Message.AppName, context.Message.EntityType, context.Message.EntityId, "Validation failed"),
                cancellationToken);
        }
    }
}
