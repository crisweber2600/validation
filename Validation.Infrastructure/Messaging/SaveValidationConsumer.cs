using MassTransit;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using Validation.Domain.Events;
using Validation.Domain.Validation;
using Validation.Infrastructure.Repositories;

namespace Validation.Infrastructure.Messaging;

/// <summary>
/// Consumes <see cref="SaveRequested"/> events and logs the metrics and
/// validation result. A span is created for each consume operation to aid
/// tracing message flow failures.
/// </summary>
public class SaveValidationConsumer<T> : IConsumer<SaveRequested>
{
    private readonly IValidationPlanProvider _planProvider;
    private readonly ISaveAuditRepository _repository;
    private readonly SummarisationValidator _validator;
    private readonly ILogger<SaveValidationConsumer<T>> _logger;
    private readonly ActivitySource _activitySource;

    public SaveValidationConsumer(
        IValidationPlanProvider planProvider,
        ISaveAuditRepository repository,
        SummarisationValidator validator,
        ILogger<SaveValidationConsumer<T>> logger,
        ActivitySource activitySource)
    {
        _planProvider = planProvider;
        _repository = repository;
        _validator = validator;
        _logger = logger;
        _activitySource = activitySource;
    }

    public async Task Consume(ConsumeContext<SaveRequested> context)
    {
        using var activity = _activitySource.StartActivity("SaveValidation.Consume");

        var last = await _repository.GetLastAsync(context.Message.Id, context.CancellationToken);
        var metric = new Random().Next(0, 100);
        var rules = _planProvider.GetRules<T>();
        var isValid = _validator.Validate(last?.Metric ?? 0m, metric, rules);

        _logger.LogInformation(
            "Validating {EntityId} previous {PreviousMetric} new {Metric} valid {Valid}",
            context.Message.Id,
            last?.Metric,
            metric,
            isValid);

        var audit = new SaveAudit
        {
            Id = Guid.NewGuid(),
            EntityId = context.Message.Id,
            IsValid = isValid,
            Metric = metric
        };

        await _repository.AddAsync(audit, context.CancellationToken);
        await context.Publish(new SaveValidated<T>(context.Message.Id, audit.Id));
    }
}