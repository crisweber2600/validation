using System.Net.Http.Json;

namespace Validation.Infrastructure.Pipeline;

public class HttpGatherService : IGatherService
{
    private readonly HttpClient _httpClient;
    private readonly HttpGatherOptions _options;

    public HttpGatherService(HttpClient httpClient, HttpGatherOptions options)
    {
        _httpClient = httpClient;
        _options = options;
    }

    public async Task<IEnumerable<T>> GatherAsync<T>(CancellationToken cancellationToken = default)
    {
        var items = await _httpClient.GetFromJsonAsync<IEnumerable<T>>(_options.Url, cancellationToken);
        return items ?? Enumerable.Empty<T>();
    }
}

public class HttpGatherOptions
{
    public string Url { get; set; } = string.Empty;
}
