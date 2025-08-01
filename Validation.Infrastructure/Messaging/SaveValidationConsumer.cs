using MassTransit;
using Validation.Domain.Events;
using Validation.Domain.Validation;
using Validation.Infrastructure.Repositories;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace Validation.Infrastructure.Messaging;

/// <summary>
/// Consumes <see cref="SaveRequested"/> messages and logs validation results.
/// The emitted Serilog entries and OpenTelemetry spans help trace message flow
/// and diagnose failures during validation.
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

    public async Task Consume(ConsumeContext<SaveRequested> context)
    {
        using var activity = _activitySource.StartActivity("Consume SaveRequested");
        var last = await _repository.GetLastAsync(context.Message.Id, context.CancellationToken);
        var metric = new Random().Next(0, 100);
        var rules = _planProvider.GetRules<T>();
        var isValid = _validator.Validate(last?.Metric ?? 0m, metric, rules);

        _logger.LogInformation("Validation for {Id}: previous={PrevMetric} new={Metric} valid={Valid}",
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
    }
}