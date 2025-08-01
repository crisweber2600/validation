using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Validation.Domain.Validation;
using Validation.Infrastructure.Repositories;

namespace Validation.Infrastructure.Metrics;

public class PipelineOrchestrator<T>
{
    private readonly IEnumerable<IMetricsGatherer> _gatherers;
    private readonly ISummarisationService _summariser;
    private readonly IValidationPlanProvider _planProvider;
    private readonly SummarisationValidator _validator;
    private readonly ISaveAuditRepository _repository;
    private readonly ILogger<PipelineOrchestrator<T>> _logger;

    public PipelineOrchestrator(
        IEnumerable<IMetricsGatherer> gatherers,
        ISummarisationService summariser,
        IValidationPlanProvider planProvider,
        SummarisationValidator validator,
        ISaveAuditRepository repository,
        ILogger<PipelineOrchestrator<T>> logger)
    {
        _gatherers = gatherers;
        _summariser = summariser;
        _planProvider = planProvider;
        _validator = validator;
        _repository = repository;
        _logger = logger;
    }

    public async Task RunAsync(Guid entityId, CancellationToken cancellationToken = default)
    {
        var metricLists = await Task.WhenAll(_gatherers.Select(g => g.GatherAsync(cancellationToken)));
        var metrics = metricLists.SelectMany(m => m).ToList();
        var summary = _summariser.Summarise(metrics);

        var plan = _planProvider.GetPlan(typeof(T));
        var previous = await _repository.GetLastAsync(entityId, cancellationToken);
        var isValid = _validator.Validate(previous?.Metric ?? 0m, summary, plan);

        var audit = new SaveAudit
        {
            Id = Guid.NewGuid(),
            EntityId = entityId,
            Metric = summary,
            IsValid = isValid
        };

        await _repository.AddAsync(audit, cancellationToken);
        _logger.LogInformation("Pipeline processed entity {Entity} with metric {Metric} valid={Valid}", entityId, summary, isValid);
    }
}
