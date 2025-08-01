using MassTransit;
using Validation.Domain.Events;
using Validation.Domain.Validation;
using Validation.Infrastructure.Repositories;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace Validation.Infrastructure.Messaging;

public class DeleteValidationConsumer<T> : IConsumer<DeleteRequested>
{
    private readonly IValidationPlanProvider _planProvider;
    private readonly SummarisationValidator _validator;
    private readonly ILogger<DeleteValidationConsumer<T>> _logger;
    private readonly ActivitySource _activitySource;

    /// <summary>
    /// Creates the consumer responsible for validating delete requests.
    /// </summary>
    public DeleteValidationConsumer(IValidationPlanProvider planProvider, SummarisationValidator validator, ILogger<DeleteValidationConsumer<T>> logger, ActivitySource activitySource)
    {
        _planProvider = planProvider;
        _validator = validator;
        _logger = logger;
        _activitySource = activitySource;
    }

    public Task Consume(ConsumeContext<DeleteRequested> context)
    {
        using var activity = _activitySource.StartActivity("DeleteValidationConsumer.Consume", ActivityKind.Consumer);
        var rules = _planProvider.GetRules<T>();
        _validator.Validate(0, 0, rules);
        _logger.LogInformation("Validated delete for entity {EntityId}", context.Message.Id);
        return Task.CompletedTask;
    }
}