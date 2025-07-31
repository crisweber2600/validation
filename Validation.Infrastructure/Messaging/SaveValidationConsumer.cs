using MassTransit;
using Validation.Domain.Events;
using Validation.Domain.Validation;
using Validation.Infrastructure.Repositories;

namespace Validation.Infrastructure.Messaging;

public class SaveValidationConsumer<T> : IConsumer<SaveRequested>
{
    private readonly IValidationPlanProvider _planProvider;
    private readonly ISaveAuditRepository _repository;
    private readonly SummarisationValidator _validator;

    public SaveValidationConsumer(IValidationPlanProvider planProvider, ISaveAuditRepository repository, SummarisationValidator validator)
    {
        _planProvider = planProvider;
        _repository = repository;
        _validator = validator;
    }

    public async Task Consume(ConsumeContext<SaveRequested> context)
    {
        var last = await _repository.GetLastAsync(context.Message.Id, context.CancellationToken);
        var metric = new Random().Next(0, 100);
        var rules = _planProvider.GetRules<T>();
        var validated = _validator.Validate(last?.Metric ?? 0m, metric, rules);

        await context.Publish(new SaveValidated(context.Message.Id, validated));
    }
}