using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Validation.Infrastructure;

public class InMemoryMetricGatherer : IMetricGatherer
{
    private readonly List<decimal> _values = new();

    public void Add(decimal value) => _values.Add(value);

    public Task<IEnumerable<decimal>> GatherAsync(CancellationToken cancellationToken = default)
        => Task.FromResult<IEnumerable<decimal>>(_values);
}
