using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Validation.Infrastructure.Metrics;

public class InMemoryMetricsGatherer : IMetricsGatherer
{
    private readonly ConcurrentQueue<decimal> _metrics = new();

    public void Add(decimal value) => _metrics.Enqueue(value);

    public Task<IEnumerable<decimal>> GatherAsync(CancellationToken cancellationToken = default)
    {
        var list = new List<decimal>();
        while (_metrics.TryDequeue(out var v))
        {
            list.Add(v);
        }
        return Task.FromResult<IEnumerable<decimal>>(list);
    }
}
