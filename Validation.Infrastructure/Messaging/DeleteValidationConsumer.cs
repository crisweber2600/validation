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

    public Task Consume(ConsumeContext<DeleteRequested> context)
    {
        var plan = _planProvider.GetPlan(typeof(T));
        // execute manual rules with zero metrics since delete; actual logic omitted
        _validator.Validate(0, 0, plan.Rules);
        return Task.CompletedTask;
    }
}