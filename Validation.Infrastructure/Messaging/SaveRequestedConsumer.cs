using MassTransit;
using Validation.Domain.Events;
using Validation.Domain.Validation;
using Validation.Infrastructure.Repositories;
using Validation.Infrastructure;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace Validation.Infrastructure.Messaging;

/// <summary>
/// Validates incoming save requests using a single rule.
/// Logging of metrics and spans help pinpoint validation errors.
/// </summary>
public class SaveRequestedConsumer : IConsumer<SaveRequested>
{
    private readonly ISaveAuditRepository _repository;
    private readonly IValidationRule _rule;
    private decimal _previousMetric;
    private readonly ILogger<SaveRequestedConsumer> _logger;
    private readonly ActivitySource _activitySource;

    public SaveRequestedConsumer(ISaveAuditRepository repository, IValidationRule rule, ILogger<SaveRequestedConsumer> logger, ActivitySource activitySource)
    {
        _repository = repository;
        _rule = rule;
        _logger = logger;
        _activitySource = activitySource;
    }

    public async Task Consume(ConsumeContext<SaveRequested> context)
    {
        using var activity = _activitySource.StartActivity("Consume SaveRequested(single)");
        var metric = new Random().Next(0, 100); // simulate metric
        var isValid = _rule.Validate(_previousMetric, metric);
        _logger.LogInformation(
            "Save requested for {Id}: previous={Prev} new={Metric} valid={Valid}",
            context.Message.Id, _previousMetric, metric, isValid);
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
