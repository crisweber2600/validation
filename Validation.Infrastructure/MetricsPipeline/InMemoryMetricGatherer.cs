namespace Validation.Infrastructure;

public class InMemoryMetricGatherer : IMetricGatherer
{
    private readonly IEnumerable<decimal> _metrics;

    public InMemoryMetricGatherer(IEnumerable<decimal> metrics)
    {
        _metrics = metrics;
    }

    public Task<IEnumerable<decimal>> GatherAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_metrics);
    }
}
