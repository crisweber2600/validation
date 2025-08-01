using MassTransit;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using Validation.Domain.Events;
using Validation.Domain.Validation;
using Validation.Infrastructure.Repositories;

namespace Validation.Infrastructure.Messaging;

/// <summary>
/// Validates delete operations and traces the consume flow for diagnostics.
/// </summary>
public class DeleteValidationConsumer<T> : IConsumer<DeleteRequested>
{
    private readonly IValidationPlanProvider _planProvider;
    private readonly SummarisationValidator _validator;
    private readonly ILogger<DeleteValidationConsumer<T>> _logger;
    private readonly ActivitySource _activitySource;

    public DeleteValidationConsumer(
        IValidationPlanProvider planProvider,
        SummarisationValidator validator,
        ILogger<DeleteValidationConsumer<T>> logger,
        ActivitySource activitySource)
    {
        _planProvider = planProvider;
        _validator = validator;
        _logger = logger;
        _activitySource = activitySource;
    }

    public Task Consume(ConsumeContext<DeleteRequested> context)
    {
        using var activity = _activitySource.StartActivity("DeleteValidation.Consume");

        var rules = _planProvider.GetRules<T>();
        // execute manual rules with zero metrics since delete; actual logic omitted
        _validator.Validate(0, 0, rules);
        _logger.LogInformation("Validated delete request for entity {EntityId}", context.Message.Id);
        return Task.CompletedTask;
    }
}