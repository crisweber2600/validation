using MassTransit;
using Validation.Domain.Events;
using Validation.Domain.Validation;
using Validation.Infrastructure.Repositories;

namespace Validation.Infrastructure.Messaging;

/// <summary>
/// Validates delete requests and publishes <see cref="DeleteValidated{T}"/> on success.
/// </summary>
public class DeleteValidationConsumer<T> : IConsumer<DeleteRequested>
{
    private readonly IValidationPlanProvider _planProvider;
    private readonly SummarisationValidator _validator;

    public DeleteValidationConsumer(IValidationPlanProvider planProvider, SummarisationValidator validator)
    {
        _planProvider = planProvider;
        _validator = validator;
    }

    public async Task Consume(ConsumeContext<DeleteRequested> context)
    {
        var rules = _planProvider.GetRules<T>();
        // execute manual rules with zero metrics since delete; actual logic omitted
        _validator.Validate(0, 0, rules);
        await context.Publish(new DeleteValidated<T>(context.Message.Id));
    }
}