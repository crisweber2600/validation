using MassTransit;
using Validation.Domain.Events;
using Validation.Domain.Validation;
using Validation.Infrastructure.Repositories;

namespace Validation.Infrastructure.Messaging;

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
        // execute manual rules with zero metrics since delete
        var isValid = _validator.Validate(0, 0, rules);
        await context.Publish(new DeleteValidated(context.Message.Id, isValid));
    }
}