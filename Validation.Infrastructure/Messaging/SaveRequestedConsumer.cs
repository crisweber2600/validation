using MassTransit;
using Validation.Domain.Events;
using Validation.Domain.Validation;
using Validation.Infrastructure.Repositories;
using Validation.Infrastructure;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace Validation.Infrastructure.Messaging;

/// <summary>
/// Validates manual save requests while logging and tracing the message lifecycle.
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

    /// <summary>
    /// Validates the incoming save request and publishes the outcome while capturing logs and a span.
    /// </summary>
    public async Task Consume(ConsumeContext<SaveRequested> context)
    {
        using var activity = _activitySource.StartActivity("SaveRequestedConsumer.Consume");
        var metric = new Random().Next(0, 100); // simulate metric
        var previous = _previousMetric;
        var isValid = _rule.Validate(previous, metric);
        _previousMetric = metric;
        _logger.LogInformation("Validating save request for {EntityId}. Previous {Prev} New {Metric} Result {Result}", context.Message.Id, previous, metric, isValid);
        var audit = new SaveAudit
        {
            Id = Guid.NewGuid(),
            EntityId = context.Message.Id,
            IsValid = isValid,
            Metric = metric
        };
        await _repository.AddAsync(audit, context.CancellationToken);
        await context.Publish(new SaveValidated(context.Message.Id, isValid, metric), context.CancellationToken);
        _logger.LogInformation("Published SaveValidated for {EntityId} with audit {AuditId}", context.Message.Id, audit.Id);
    }
}
