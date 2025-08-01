using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Validation.Infrastructure.Metrics;

public class HttpMetricsGatherer : IMetricsGatherer
{
    private readonly HttpClient _client;
    private readonly string _url;

    public HttpMetricsGatherer(HttpClient client, string url)
    {
        _client = client;
        _url = url;
    }

    public async Task<IEnumerable<decimal>> GatherAsync(CancellationToken cancellationToken = default)
    {
        var json = await _client.GetStringAsync(_url, cancellationToken);
        var data = JsonSerializer.Deserialize<List<decimal>>(json) ?? new();
        return data;
    }
}
