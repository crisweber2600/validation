using MassTransit;
using Validation.Domain.Events;
using Validation.Domain.Validation;
using Validation.Domain.Providers;
using Validation.Infrastructure.Repositories;

namespace Validation.Infrastructure.Messaging;

public class SaveValidationConsumer<T> : IConsumer<SaveRequested>
{
    private readonly IValidationPlanProvider _planProvider;
    private readonly ISaveAuditRepository _repository;
    private readonly SummarisationValidator _validator;
    private readonly IEntityIdProvider _entityIdProvider;
    private readonly IApplicationNameProvider _applicationNameProvider;

    public SaveValidationConsumer(
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

    public async Task Consume(ConsumeContext<SaveRequested> context)
    {
        var entityId = context.Message.Id.ToString();
        var last = await _repository.GetLastAsync(entityId, context.CancellationToken);
        var metric = new Random().Next(0, 100);
        var rules = _planProvider.GetRules<T>();
        var isValid = _validator.Validate(last?.Metric ?? 0m, metric, rules);

        var audit = new SaveAudit
        {
            Id = Guid.NewGuid().ToString(),
            EntityId = entityId,
            EntityType = typeof(T).Name,
            ApplicationName = _applicationNameProvider.GetApplicationName(),
            IsValid = isValid,
            Metric = metric,
            OperationType = "Save",
            CorrelationId = context.CorrelationId?.ToString(),
            ValidationDetails = isValid ? null : "Validation failed based on summarisation rules",
            AppliedRules = string.Join(", ", rules.Select(r => r.ToString())),
            TriggeredBy = "SaveValidationConsumer"
        };

        await _repository.AddAsync(audit, context.CancellationToken);
        await context.Publish(new SaveValidated<T>(context.Message.Id, Guid.Parse(audit.Id)));
    }
}