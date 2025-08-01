using MassTransit;
using Validation.Domain.Events;
using Validation.Domain.Validation;
using Validation.Infrastructure.Repositories;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace Validation.Infrastructure.Messaging;

/// <summary>
/// Consumes <see cref="SaveRequested"/> messages and validates the payload.
/// Logs metrics and validation result and starts an OpenTelemetry span for the
/// operation to diagnose message flow and failures.
/// </summary>
public class SaveValidationConsumer<T> : IConsumer<SaveRequested>
{
    private readonly IValidationPlanProvider _planProvider;
    private readonly ISaveAuditRepository _repository;
    private readonly SummarisationValidator _validator;
    private readonly ILogger<SaveValidationConsumer<T>> _logger;
    private readonly ActivitySource _activitySource;

    public SaveValidationConsumer(IValidationPlanProvider planProvider, ISaveAuditRepository repository, SummarisationValidator validator, ILogger<SaveValidationConsumer<T>> logger, ActivitySource activitySource)
    {
        _planProvider = planProvider;
        _repository = repository;
        _validator = validator;
        _logger = logger;
        _activitySource = activitySource;
    }

    /// <summary>
    /// Handles the save request while emitting logs and a tracing span so that
    /// message processing can be correlated across services.
    /// </summary>
    public async Task Consume(ConsumeContext<SaveRequested> context)
    {
        using var activity = _activitySource.StartActivity("SaveValidationConsumer.Consume");
        var last = await _repository.GetLastAsync(context.Message.Id, context.CancellationToken);
        var metric = new Random().Next(0, 100);
        var rules = _planProvider.GetRules<T>();
        var isValid = _validator.Validate(last?.Metric ?? 0m, metric, rules);

        _logger.LogInformation("Validating entity {EntityId}. Previous {PreviousMetric} New {Metric} Result {Result}",
            context.Message.Id, last?.Metric, metric, isValid);

        var audit = new SaveAudit
        {
            Id = Guid.NewGuid(),
            EntityId = context.Message.Id,
            IsValid = isValid,
            Metric = metric
        };

        await _repository.AddAsync(audit, context.CancellationToken);
        await context.Publish(new SaveValidated<T>(context.Message.Id, audit.Id));
        _logger.LogInformation("Published SaveValidated for {EntityId} with audit {AuditId}", context.Message.Id, audit.Id);
    }
}