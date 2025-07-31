using MassTransit;
using Validation.Domain.Events;
using Validation.Infrastructure.Services;
using Validation.Infrastructure.Repositories;

namespace Validation.Infrastructure.Messaging;

public class DeleteValidationConsumer<T> : IConsumer<DeleteRequested>
{
    private readonly IValidationPlanProvider _planProvider;
    private readonly ISaveAuditRepository _repository;
    private readonly SummarisationValidator _validator;

    public DeleteValidationConsumer(
        IValidationPlanProvider planProvider,
        ISaveAuditRepository repository,
        SummarisationValidator validator)
    {
        _planProvider = planProvider;
        _repository = repository;
        _validator = validator;
    }

    public async Task Consume(ConsumeContext<DeleteRequested> context)
    {
        var lastAudit = await _repository.GetLastForEntityAsync(context.Message.Id, context.CancellationToken);
        var metric = lastAudit?.Metric ?? 0m;
        var plan = _planProvider.GetPlan<T>();
        _ = _validator.Validate(metric, metric, plan);
        await _repository.DeleteAsync(context.Message.Id, context.CancellationToken);
    }
}
