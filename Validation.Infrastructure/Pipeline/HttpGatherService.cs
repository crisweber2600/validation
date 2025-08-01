using System.Net.Http.Json;

namespace Validation.Infrastructure.Pipeline;

public class HttpGatherService : IGatherService
{
    private readonly HttpClient _client;
    private readonly string _endpoint;

    public HttpGatherService(HttpClient client, string endpoint)
    {
        _client = client;
        _endpoint = endpoint;
    }

    public async Task<IEnumerable<T>> GatherAsync<T>(CancellationToken cancellationToken = default)
    {
        var data = await _client.GetFromJsonAsync<IEnumerable<T>>(_endpoint, cancellationToken);
        return data ?? Array.Empty<T>();
    }
}
