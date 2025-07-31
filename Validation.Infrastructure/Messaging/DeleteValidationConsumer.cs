using MassTransit;
using Validation.Domain.Events;
using Validation.Domain.Validation;
using Validation.Infrastructure.Repositories;

namespace Validation.Infrastructure.Messaging;

public class DeleteValidationConsumer<T> : IConsumer<DeleteRequested>
{
    private readonly IValidationPlanProvider _planProvider;
    private readonly ISummarisationValidator _validator;

    public DeleteValidationConsumer(IValidationPlanProvider planProvider, ISummarisationValidator validator)
    {
        _planProvider = planProvider;
        _validator = validator;
    }

    public Task Consume(ConsumeContext<DeleteRequested> context)
    {
        var rules = _planProvider.GetRules<T>();
        // execute manual rules with zero metrics since delete; actual logic omitted
        _validator.Validate(0, 0, rules);
        return Task.CompletedTask;
    }
}