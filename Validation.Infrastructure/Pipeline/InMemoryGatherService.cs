using System.Collections.Concurrent;

namespace Validation.Infrastructure.Pipeline;

public class InMemoryGatherService : IGatherService
{
    private readonly ConcurrentDictionary<Type, IList<object>> _storage = new();

    public void Add<T>(T item)
    {
        var list = _storage.GetOrAdd(typeof(T), _ => new List<object>());
        lock (list)
        {
            list.Add(item!);
        }
    }

    public Task<IEnumerable<T>> GatherAsync<T>(CancellationToken cancellationToken = default)
    {
        if (_storage.TryGetValue(typeof(T), out var list))
        {
            lock (list)
            {
                var result = list.Cast<T>().ToList();
                list.Clear();
                return Task.FromResult<IEnumerable<T>>(result);
            }
        }
        return Task.FromResult<IEnumerable<T>>(Array.Empty<T>());
    }
}
