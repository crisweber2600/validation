using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Validation.Infrastructure.Pipeline;

public class InMemoryMetricGatherer : IMetricGatherer
{
    private readonly IEnumerable<decimal> _metrics;

    public InMemoryMetricGatherer(IEnumerable<decimal> metrics)
    {
        _metrics = metrics;
    }

    public Task<IEnumerable<decimal>> GatherMetricsAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_metrics);
    }
}
