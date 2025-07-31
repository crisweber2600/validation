using MassTransit;
using Validation.Domain.Events;
using Validation.Domain.Validation;
using Validation.Infrastructure.Repositories;

namespace Validation.Infrastructure.Messaging;

public class SaveRequestedConsumer : IConsumer<SaveRequested>
{
    private readonly ISaveAuditRepository _repository;
    private readonly IValidationRule _rule;
    private decimal _previousMetric;

    public SaveRequestedConsumer(ISaveAuditRepository repository, IValidationRule rule)
    {
        _repository = repository;
        _rule = rule;
    }

    public async Task Consume(ConsumeContext<SaveRequested> context)
    {
        var metric = new Random().Next(0, 100); // simulate metric
        var isValid = _rule.Validate(_previousMetric, metric);
        _previousMetric = metric;
        var audit = new SaveAudit
        {
            Id = Guid.NewGuid(),
            EntityId = context.Message.Id,
            IsValid = isValid,
            Metric = metric
        };
        await _repository.AddAsync(audit, context.CancellationToken);
        await context.Publish(new SaveValidated(context.Message.Id, isValid, metric), context.CancellationToken);
    }
}
