using MassTransit;
using Validation.Domain.Events;
using Validation.Infrastructure.Services;
using Validation.Domain.Validation;
using Validation.Infrastructure.Repositories;
using Validation.Infrastructure;

namespace Validation.Infrastructure.Messaging;

public class SaveValidationConsumer<T> : IConsumer<SaveRequested>
{
    private readonly IValidationPlanProvider _planProvider;
    private readonly ISaveAuditRepository _repository;
    private readonly SummarisationValidator _validator;

    public SaveValidationConsumer(
        IValidationPlanProvider planProvider,
        ISaveAuditRepository repository,
        SummarisationValidator validator)
    {
        _planProvider = planProvider;
        _repository = repository;
        _validator = validator;
    }

    public async Task Consume(ConsumeContext<SaveRequested> context)
    {
        var lastAudit = await _repository.GetLastForEntityAsync(context.Message.Id, context.CancellationToken);
        var previousMetric = lastAudit?.Metric ?? 0m;
        var metric = new Random().Next(0, 100); // simulate metric generation

        var plan = _planProvider.GetPlan<T>();
        var isValid = _validator.Validate(previousMetric, metric, plan);

        var audit = new SaveAudit
        {
            Id = Guid.NewGuid(),
            EntityId = context.Message.Id,
            IsValid = isValid,
            Metric = metric
        };
        await _repository.AddAsync(audit, context.CancellationToken);
        await context.Publish(new SaveValidated<T>(context.Message.Id));
    }
}
