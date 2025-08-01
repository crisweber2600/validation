using MassTransit;
using Validation.Domain.Events;
using Validation.Domain.Validation;
using Validation.Infrastructure.Repositories;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace Validation.Infrastructure.Messaging;

public class SaveValidationConsumer<T> : IConsumer<SaveRequested>
{
    private readonly IValidationPlanProvider _planProvider;
    private readonly ISaveAuditRepository _repository;
    private readonly SummarisationValidator _validator;
    private readonly ILogger<SaveValidationConsumer<T>> _logger;
    private readonly ActivitySource _activitySource;

    /// <summary>
    /// Creates a new consumer for <see cref="SaveRequested"/> events.
    /// </summary>
    /// <param name="planProvider">Provides validation rules.</param>
    /// <param name="repository">Repository used to store audit records.</param>
    /// <param name="validator">Validator that executes the rules.</param>
    /// <param name="logger">Serilog logger used to record validation results.</param>
    /// <param name="activitySource">Source used to create tracing activities.</param>
    public SaveValidationConsumer(IValidationPlanProvider planProvider, ISaveAuditRepository repository, SummarisationValidator validator, ILogger<SaveValidationConsumer<T>> logger, ActivitySource activitySource)
    {
        _planProvider = planProvider;
        _repository = repository;
        _validator = validator;
        _logger = logger;
        _activitySource = activitySource;
    }

    /// <summary>
    /// Validates a save request and publishes the result. Logs the incoming entity ID
    /// and validation outcome, and creates a tracing span for diagnostics.
    /// </summary>
    public async Task Consume(ConsumeContext<SaveRequested> context)
    {
        using var activity = _activitySource.StartActivity("SaveValidationConsumer.Consume", ActivityKind.Consumer);
        var last = await _repository.GetLastAsync(context.Message.Id, context.CancellationToken);
        var metric = new Random().Next(0, 100);
        var rules = _planProvider.GetRules<T>();
        var isValid = _validator.Validate(last?.Metric ?? 0m, metric, rules);

        _logger.LogInformation("Validating entity {EntityId} from {Previous} to {Current}: {Result}",
            context.Message.Id, last?.Metric, metric, isValid ? "passed" : "failed");

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