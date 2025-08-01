using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

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
        var response = await _client.GetStringAsync(_url, cancellationToken);
        return decimal.TryParse(response, out var value)
            ? new[] { value }
            : Array.Empty<decimal>();
    }
}
