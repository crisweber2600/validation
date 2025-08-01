using System.Collections.Concurrent;

namespace Validation.Infrastructure.Metrics.Pipeline;

public class InMemoryMetricGatherer : IMetricGatherer
{
    private readonly ConcurrentQueue<double> _metrics = new();

    public void AddMetric(double value) => _metrics.Enqueue(value);

    public Task<IEnumerable<double>> GatherAsync(CancellationToken cancellationToken = default)
    {
        var list = new List<double>();
        while (_metrics.TryDequeue(out var value))
        {
            list.Add(value);
        }
        return Task.FromResult<IEnumerable<double>>(list);
    }
}
