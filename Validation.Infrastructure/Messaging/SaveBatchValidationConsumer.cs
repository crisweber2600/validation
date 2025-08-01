using MassTransit;
using Validation.Domain;
using Validation.Domain.Events;
using Validation.Domain.Validation;
using Validation.Infrastructure.Repositories;
using ValidationFlow.Messages;

namespace Validation.Infrastructure.Messaging;

public class SaveBatchValidationConsumer<T> : IConsumer<SaveBatchRequested<T>> where T : class
{
    private readonly IValidationPlanProvider  _plans;
    private readonly ISaveAuditRepository     _audits;
    private readonly IManualValidatorService  _manual;
    private readonly IEntityIdProvider        _ids;
    private readonly IApplicationNameProvider _app;

    public SaveBatchValidationConsumer(
        IValidationPlanProvider plans,
        ISaveAuditRepository audits,
        IManualValidatorService manual,
        IEntityIdProvider ids,
        IApplicationNameProvider app)
    {
        _plans  = plans;
        _audits = audits;
        _manual = manual;
        _ids    = ids;
        _app    = app;
    }

    public async Task Consume(ConsumeContext<SaveBatchRequested<T>> ctx)
    {
        var list = ctx.Message.Entities.ToList();
        if (list.Any(item => !_manual.Validate(item!)))
        {
            await ctx.Publish(new ValidationOperationFailed(
                _ids.GetId(list.First()), typeof(T).Name, "SaveBatch", "Manual rule failed"));
            return;
        }

        var plan = _plans.GetPlan(typeof(T));
        var selector = plan.MetricSelector ?? (_ => 0m);

        var seqOk = await SequenceValidator.ValidateBatchAsync(
            list, x => selector(x!), _audits, _ids,
            plan.ThresholdValue ?? 0m,
            plan.ThresholdType  ?? ThresholdType.RawDifference,
            null,
            ctx.CancellationToken);

        if (!seqOk)
        {
            await ctx.Publish(new ValidationOperationFailed(
                _ids.GetId(list.First()), typeof(T).Name, "SaveBatch", "Sequence validation failed"));
            return;
        }

        var metric = list.Sum(x => selector(x!));

        var audit = new SaveAudit
        {
            EntityId        = _ids.GetId(list.First()),
            ApplicationName = _app.ApplicationName,
            BatchSize       = list.Count,
            IsValid         = true,
            Metric          = metric
        };
        await _audits.AddAsync(audit, ctx.CancellationToken);

        await ctx.Publish(new Validation.Domain.Events.SaveValidated<T>(ctx.Message.CorrelationId, audit.Id));
    }
}
