namespace Validation.Infrastructure.Pipeline;

public class InMemoryGatherService : IGatherService
{
    private readonly Dictionary<Type, IList<object>> _data = new();

    public void AddRange<T>(IEnumerable<T> items)
    {
        _data[typeof(T)] = items.Cast<object>().ToList();
    }

    public Task<IEnumerable<T>> GatherAsync<T>(CancellationToken cancellationToken = default)
    {
        if (_data.TryGetValue(typeof(T), out var list))
            return Task.FromResult(list.Cast<T>());
        return Task.FromResult(Enumerable.Empty<T>());
    }
}
