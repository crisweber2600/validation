using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Validation.Domain.Validation;

namespace Validation.Infrastructure.Pipeline;

public class PipelineOrchestrator
{
    private readonly IEnumerable<IMetricGatherer> _gatherers;
    private readonly ISummarisationService _summarisationService;
    private readonly SummarisationValidator _validator = new();
    private readonly List<decimal> _committed = new();
    private decimal _lastSummary;

    public PipelineOrchestrator(IEnumerable<IMetricGatherer> gatherers, ISummarisationService summarisationService)
    {
        _gatherers = gatherers;
        _summarisationService = summarisationService;
    }

    public async Task<IEnumerable<decimal>> GatherDataAsync(CancellationToken cancellationToken = default)
    {
        var results = new List<decimal>();
        foreach (var gatherer in _gatherers)
        {
            var data = await gatherer.GatherMetricsAsync(cancellationToken);
            if (data != null) results.AddRange(data);
        }
        return results;
    }

    public Task<decimal> SummariseAsync(IEnumerable<decimal> metrics, CancellationToken cancellationToken = default)
    {
        return _summarisationService.SummariseAsync(metrics, cancellationToken);
    }

    public bool Validate(decimal previous, decimal current)
    {
        var plan = new ValidationPlan(_ => current, ThresholdType.RawDifference, 10);
        return _validator.Validate(previous, current, plan);
    }

    public Task CommitResultsAsync(decimal summary, CancellationToken cancellationToken = default)
    {
        _committed.Add(summary);
        _lastSummary = summary;
        return Task.CompletedTask;
    }

    public async Task RunPipelineAsync(CancellationToken cancellationToken = default)
    {
        var data = await GatherDataAsync(cancellationToken);
        var summary = await SummariseAsync(data, cancellationToken);
        if (Validate(_lastSummary, summary))
        {
            await CommitResultsAsync(summary, cancellationToken);
        }
    }

    public IReadOnlyList<decimal> CommittedResults => _committed.AsReadOnly();
}
