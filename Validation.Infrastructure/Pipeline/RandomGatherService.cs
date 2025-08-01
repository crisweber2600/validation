namespace Validation.Infrastructure.Pipeline;

/// <summary>
/// Default gatherer that generates random metrics for testing.
/// </summary>
public class RandomGatherService : IGatherService
{
    private readonly Random _random = new();

    public Task<IEnumerable<decimal>> GatherAsync(CancellationToken ct)
    {
        var data = Enumerable.Range(0, 5).Select(_ => (decimal)_random.NextDouble() * 100).ToArray();
        return Task.FromResult<IEnumerable<decimal>>(data);
    }
}
