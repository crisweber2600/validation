using System.Net.Http;
using System.Text.Json;

namespace Validation.Infrastructure.Pipeline;

public class InMemoryGatherService : IGatherService
{
    private readonly IEnumerable<object> _items;
    public InMemoryGatherService(IEnumerable<object> items) => _items = items;
    public Task<IEnumerable<T>> GatherAsync<T>(CancellationToken ct = default)
        => Task.FromResult(_items.Cast<T>());
}

public class HttpGatherService : IGatherService
{
    private readonly HttpClient _client;
    private readonly Uri _uri;
    public HttpGatherService(HttpClient client, Uri uri)
    {
        _client = client;
        _uri = uri;
    }
    public async Task<IEnumerable<T>> GatherAsync<T>(CancellationToken ct = default)
    {
        var json = await _client.GetStringAsync(_uri, ct);
        return JsonSerializer.Deserialize<IEnumerable<T>>(json) ?? Array.Empty<T>();
    }
}
