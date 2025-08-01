using MassTransit;
using Validation.Domain.Events;
using Validation.Domain.Validation;
using Validation.Domain.Providers;
using Validation.Infrastructure.Repositories;

namespace Validation.Infrastructure.Messaging;

/// <summary>
/// Entity-aware save validation consumer that works with SaveRequested<T> containing the actual entity
/// This enables the use of IEntityIdProvider for custom entity ID extraction
/// </summary>
public class EntityAwareSaveValidationConsumer<T> : IConsumer<SaveRequested<T>>
{
    private readonly IValidationPlanProvider _planProvider;
    private readonly ISaveAuditRepository _repository;
    private readonly SummarisationValidator _validator;
    private readonly IEntityIdProvider _entityIdProvider;
    private readonly IApplicationNameProvider _applicationNameProvider;

    public EntityAwareSaveValidationConsumer(
        IValidationPlanProvider planProvider, 
        ISaveAuditRepository repository, 
        SummarisationValidator validator,
        IEntityIdProvider entityIdProvider,
        IApplicationNameProvider applicationNameProvider)
    {
        _planProvider = planProvider;
        _repository = repository;
        _validator = validator;
        _entityIdProvider = entityIdProvider;
        _applicationNameProvider = applicationNameProvider;
    }

    public async Task Consume(ConsumeContext<SaveRequested<T>> context)
    {
        // Use the entity ID provider to get the entity ID from the actual entity
        var entityGuid = _entityIdProvider.GetEntityId(context.Message.Entity);
        var entityId = entityGuid.ToString();
        
        var last = await _repository.GetLastAsync(entityId, context.CancellationToken);
        var metric = new Random().Next(0, 100);
        var rules = _planProvider.GetRules<T>();
        var isValid = _validator.Validate(last?.Metric ?? 0m, metric, rules);

        var audit = new SaveAudit
        {
            Id = Guid.NewGuid().ToString(),
            EntityId = entityId,
            EntityType = typeof(T).Name,
            ApplicationName = context.Message.App ?? _applicationNameProvider.GetApplicationName(),
            IsValid = isValid,
            Metric = metric,
            OperationType = "Save",
            CorrelationId = context.CorrelationId?.ToString(),
            ValidationDetails = isValid ? null : "Validation failed based on summarisation rules",
            AppliedRules = string.Join(", ", rules.Select(r => r.ToString())),
            TriggeredBy = "EntityAwareSaveValidationConsumer"
        };

        await _repository.AddAsync(audit, context.CancellationToken);
        await context.Publish(new SaveValidated<T>(entityGuid, Guid.Parse(audit.Id)));
    }
}