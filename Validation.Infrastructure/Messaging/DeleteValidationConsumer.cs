using MassTransit;
using ValidationFlow.Messages;
using Validation.Domain.Validation;
using Validation.Infrastructure.Repositories;

namespace Validation.Infrastructure.Messaging;

public class DeleteValidationConsumer<T> : IConsumer<DeleteRequested<T>>
{
    private readonly IValidationPlanProvider _planProvider;
    private readonly SummarisationValidator _validator;

    public DeleteValidationConsumer(IValidationPlanProvider planProvider, SummarisationValidator validator)
    {
        _planProvider = planProvider;
        _validator = validator;
    }

    public Task Consume(ConsumeContext<DeleteRequested<T>> context)
    {
        var rules = _planProvider.GetRules<T>();
        // execute manual rules with zero metrics since delete; actual logic omitted
        _validator.Validate(0, 0, rules);
        return Task.CompletedTask;
    }
}