using System.Net.Http.Json;

namespace Validation.Infrastructure.Pipeline;

public class HttpGatherService : IGatherService
{
    private readonly HttpClient _client;
    private readonly string _url;

    public HttpGatherService(HttpClient client, string url)
    {
        _client = client;
        _url = url;
    }

    public async Task<IEnumerable<double>> GatherAsync(CancellationToken cancellationToken = default)
    {
        var result = await _client.GetFromJsonAsync<IEnumerable<double>>(_url, cancellationToken);
        return result ?? Array.Empty<double>();
    }
}
