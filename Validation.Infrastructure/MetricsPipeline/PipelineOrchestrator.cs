using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Validation.Infrastructure.Repositories;
using Validation.Domain.Validation;

namespace Validation.Infrastructure;

public class PipelineOrchestrator
{
    private readonly IEnumerable<IMetricGatherer> _gatherers;
    private readonly ISummarisationService _summarisationService;
    private readonly ISaveAuditRepository _auditRepository;
    private readonly SummarisationValidator _validator;
    private readonly ValidationPlan _plan;
    private readonly Guid _entityId;

    public PipelineOrchestrator(
        IEnumerable<IMetricGatherer> gatherers,
        ISummarisationService summarisationService,
        ISaveAuditRepository auditRepository,
        SummarisationValidator validator,
        ValidationPlan plan,
        Guid entityId)
    {
        _gatherers = gatherers;
        _summarisationService = summarisationService;
        _auditRepository = auditRepository;
        _validator = validator;
        _plan = plan;
        _entityId = entityId;
    }

    public async Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        var data = await GatherDataAsync(cancellationToken);
        var summary = Summarise(data);
        var isValid = await ValidateAsync(summary, cancellationToken);
        await CommitAsync(summary, isValid, cancellationToken);
    }

    public virtual async Task<IEnumerable<decimal>> GatherDataAsync(CancellationToken cancellationToken = default)
    {
        var list = new List<decimal>();
        foreach (var g in _gatherers)
        {
            var values = await g.GatherAsync(cancellationToken);
            list.AddRange(values);
        }
        return list;
    }

    public virtual decimal Summarise(IEnumerable<decimal> metrics)
    {
        return _summarisationService.Summarise(metrics);
    }

    public virtual async Task<bool> ValidateAsync(decimal summary, CancellationToken cancellationToken = default)
    {
        var last = await _auditRepository.GetLastAsync(_entityId, cancellationToken);
        var previous = last?.Metric ?? 0m;
        return _validator.Validate(previous, summary, _plan);
    }

    public virtual async Task CommitAsync(decimal summary, bool isValid, CancellationToken cancellationToken = default)
    {
        var audit = new SaveAudit
        {
            Id = Guid.NewGuid(),
            EntityId = _entityId,
            Metric = summary,
            IsValid = isValid
        };
        await _auditRepository.AddAsync(audit, cancellationToken);
    }
}
