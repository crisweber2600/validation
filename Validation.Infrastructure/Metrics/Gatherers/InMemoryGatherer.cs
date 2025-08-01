using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Validation.Infrastructure.Metrics;

public class InMemoryGatherer : IMetricsGatherer
{
    private readonly IEnumerable<decimal> _values;

    public InMemoryGatherer(IEnumerable<decimal> values)
    {
        _values = values;
    }

    public Task<IEnumerable<decimal>> GatherAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_values);
    }
}
