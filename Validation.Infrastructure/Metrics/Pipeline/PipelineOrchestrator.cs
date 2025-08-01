namespace Validation.Infrastructure.Metrics.Pipeline;

public class PipelineOrchestrator
{
    private readonly IEnumerable<IMetricGatherer> _gatherers;
    private readonly ISummarisationService _summarisation;

    public PipelineOrchestrator(IEnumerable<IMetricGatherer> gatherers, ISummarisationService summarisation)
    {
        _gatherers = gatherers;
        _summarisation = summarisation;
    }

    public async Task RunAsync(CancellationToken cancellationToken = default)
    {
        var allMetrics = new List<double>();
        foreach (var gatherer in _gatherers)
        {
            var metrics = await gatherer.GatherAsync(cancellationToken);
            allMetrics.AddRange(metrics);
        }

        var summary = await _summarisation.SummariseAsync(allMetrics, cancellationToken);
        if (await _summarisation.ValidateAsync(summary, cancellationToken))
        {
            await _summarisation.CommitAsync(summary, cancellationToken);
        }
    }
}
