using MassTransit;
using Validation.Domain.Events;
using Validation.Domain.Validation;

namespace Validation.Infrastructure.Messaging;

public class DeleteValidationConsumer<T> : IConsumer<DeleteRequested>
{
    private readonly IValidationPlanProvider _provider;
    private readonly SummarisationValidator _validator;

    public DeleteValidationConsumer(IValidationPlanProvider provider, SummarisationValidator validator)
    {
        _provider = provider;
        _validator = validator;
    }

    public Task Consume(ConsumeContext<DeleteRequested> context)
    {
        var rules = _provider.GetRules<T>();
        _validator.Validate(rules, 0, 0);
        return Task.CompletedTask;
    }
}
