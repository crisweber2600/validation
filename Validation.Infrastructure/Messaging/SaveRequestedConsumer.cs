using MassTransit;
using ValidationFlow.Messages;
using Validation.Domain.Validation;
using Validation.Infrastructure.Repositories;
using Validation.Infrastructure;

namespace Validation.Infrastructure.Messaging;

public class SaveRequestedConsumer<T> : IConsumer<SaveRequested<T>>
{
    private readonly ISaveAuditRepository _repository;
    private readonly IValidationRule _rule;
    private decimal _previousMetric;

    public SaveRequestedConsumer(ISaveAuditRepository repository, IValidationRule rule)
    {
        _repository = repository;
        _rule = rule;
    }

    public async Task Consume(ConsumeContext<SaveRequested<T>> context)
    {
        var metric = new Random().Next(0, 100); // simulate metric
        var isValid = _rule.Validate(_previousMetric, metric);
        _previousMetric = metric;
        var audit = new SaveAudit
        {
            Id = Guid.NewGuid(),
            EntityId = context.Message.EntityId,
            IsValid = isValid,
            Metric = metric
        };
        await _repository.AddAsync(audit, context.CancellationToken);
        await context.Publish(new SaveValidated<T>(context.Message.AppName, context.Message.EntityType, context.Message.EntityId, context.Message.Payload, isValid), context.CancellationToken);
    }
}
