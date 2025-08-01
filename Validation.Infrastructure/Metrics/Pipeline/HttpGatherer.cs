using System.Net.Http.Json;

namespace Validation.Infrastructure.Metrics;

public class HttpGatherer : IMetricsGatherer
{
    private readonly HttpClient _client;
    private readonly string _url;

    public HttpGatherer(HttpClient client, string url)
    {
        _client = client;
        _url = url;
    }

    public async Task<IEnumerable<decimal>> GatherAsync(CancellationToken cancellationToken = default)
    {
        var values = await _client.GetFromJsonAsync<IEnumerable<decimal>>(_url, cancellationToken);
        return values ?? Enumerable.Empty<decimal>();
    }
}
