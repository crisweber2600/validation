using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;

namespace Validation.Infrastructure.Pipeline;

/// <summary>
/// Simple gatherer that returns a set of random data for testing purposes.
/// </summary>
public class InMemoryGatherService : IGatherService
{
    private readonly Random _random = new();

    public Task<IEnumerable<decimal>> GatherAsync(CancellationToken ct)
    {
        var data = Enumerable.Range(0, 5).Select(_ => (decimal)_random.Next(0, 100)).ToArray();
        return Task.FromResult<IEnumerable<decimal>>(data);
    }
}
