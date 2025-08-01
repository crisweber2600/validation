namespace Validation.Infrastructure.Pipeline;

internal class InMemoryGatherService : IGatherService
{
    private static readonly Random _rand = new();

    public Task<IEnumerable<decimal>> GatherAsync(CancellationToken ct)
    {
        var data = Enumerable.Range(0, 5).Select(_ => (decimal)_rand.NextDouble() * 100);
        return Task.FromResult(data);
    }
}
