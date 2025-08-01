using MassTransit;
using Validation.Domain.Events;
using Validation.Domain.Validation;
using Validation.Domain;
using Validation.Infrastructure.Repositories;

namespace Validation.Infrastructure.Messaging;

public class SaveValidationConsumer<T> : IConsumer<SaveRequested<T>> where T : class
{
    private readonly IValidationPlanProvider   _planProvider;
    private readonly ISaveAuditRepository      _auditRepo;
    private readonly IManualValidatorService   _manual;
    private readonly IEntityIdProvider         _idProvider;
    private readonly IApplicationNameProvider  _appName;

    public SaveValidationConsumer(
        IValidationPlanProvider planProvider,
        ISaveAuditRepository auditRepo,
        IManualValidatorService manual,
        IEntityIdProvider idProvider,
        IApplicationNameProvider appName)
    {
        _planProvider = planProvider;
        _auditRepo    = auditRepo;
        _manual       = manual;
        _idProvider   = idProvider;
        _appName      = appName;
    }

    public async Task Consume(ConsumeContext<SaveRequested<T>> ctx)
    {
        var entity = ctx.Message.Entity;
        var plan   = _planProvider.GetPlan(typeof(T));

        // Manual rules
        if (!_manual.Validate(entity))
        {
            await ctx.Publish(new ValidationOperationFailed(
                _idProvider.GetId(entity),
                typeof(T).Name,
                "Save",
                "Manual rule failed"));
            return;
        }

        // Sequence validation
        var seqOk = await SequenceValidator.ValidateAsync(
            entity,
            plan.MetricSelector!,
            _auditRepo,
            _idProvider,
            plan.ThresholdValue ?? 0m,
            plan.ThresholdType ?? ThresholdType.RawDifference,
            ctx.CancellationToken);

        if (!seqOk)
        {
            await ctx.Publish(new ValidationOperationFailed(
                _idProvider.GetId(entity),
                typeof(T).Name,
                "Save",
                "Sequence validation failed"));
            return;
        }

        // Summarisation / threshold validation
        var last     = await _auditRepo.GetLastAsync(_idProvider.GetId(entity), ctx.CancellationToken);
        var previous = last?.Metric ?? 0m;
        var metric   = plan.MetricSelector!(entity);

        var summariser = new SummarisationValidator();
        if (!summariser.Validate(previous, metric, plan))
        {
            await ctx.Publish(new ValidationOperationFailed(
                _idProvider.GetId(entity),
                typeof(T).Name,
                "Save",
                "Threshold validation failed"));
            return;
        }

        // Record audit
        var audit = new SaveAudit
        {
            EntityId        = _idProvider.GetId(entity),
            ApplicationName = _appName.ApplicationName,
            BatchSize       = 1,
            IsValid         = true,
            Metric          = metric
        };
        await _auditRepo.AddAsync(audit, ctx.CancellationToken);

        await ctx.Publish(new SaveValidated<T>(_idProvider.GetId(entity), audit.Id));
    }
}
