using System;
using System.Linq;
namespace Validation.Infrastructure.Pipeline;

/// <summary>
/// Default gather service that generates random metrics for testing.
/// </summary>
public class InMemoryGatherService : IGatherService
{
    private readonly Random _random = new();

    public Task<IEnumerable<decimal>> GatherAsync(CancellationToken ct)
    {
        var values = Enumerable.Range(0, 5).Select(_ => (decimal)_random.Next(0, 100)).ToArray();
        return Task.FromResult<IEnumerable<decimal>>(values);
    }
}
