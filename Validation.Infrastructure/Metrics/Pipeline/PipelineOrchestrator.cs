using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Validation.Domain.Validation;

namespace Validation.Infrastructure.Metrics;

public interface ISummaryGatherer
{
    Task<IEnumerable<decimal>> GatherAsync(CancellationToken cancellationToken = default);
}

public interface ISummarisationService
{
    Task<decimal> SummariseAsync(IEnumerable<decimal> values, CancellationToken cancellationToken = default);
}

public interface ISummaryCommitter
{
    Task CommitAsync(decimal summary, CancellationToken cancellationToken = default);
}

public class InMemoryGatherer : ISummaryGatherer
{
    private readonly IEnumerable<decimal> _values;
    public InMemoryGatherer(IEnumerable<decimal> values) => _values = values;
    public Task<IEnumerable<decimal>> GatherAsync(CancellationToken cancellationToken = default) => Task.FromResult(_values);
}

public class HttpGatherer : ISummaryGatherer
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
        var json = await _client.GetStringAsync(_url, cancellationToken);
        var values = System.Text.Json.JsonSerializer.Deserialize<decimal[]>(json) ?? Array.Empty<decimal>();
        return values;
    }
}

public class PipelineOrchestrator
{
    private readonly IEnumerable<ISummaryGatherer> _gatherers;
    private readonly ISummarisationService _summariser;
    private readonly SummarisationValidator _validator;
    private readonly ValidationPlan _plan;
    private readonly ISummaryCommitter _committer;
    private decimal? _last;

    public PipelineOrchestrator(IEnumerable<ISummaryGatherer> gatherers, ISummarisationService summariser,
        SummarisationValidator validator, ValidationPlan plan, ISummaryCommitter committer)
    {
        _gatherers = gatherers;
        _summariser = summariser;
        _validator = validator;
        _plan = plan;
        _committer = committer;
    }

    public async Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        var all = new List<decimal>();
        foreach (var g in _gatherers)
        {
            var vals = await g.GatherAsync(cancellationToken);
            if (vals != null) all.AddRange(vals);
        }

        var summary = await _summariser.SummariseAsync(all, cancellationToken);
        var prev = _last ?? summary;
        if (_validator.Validate(prev, summary, _plan))
        {
            await _committer.CommitAsync(summary, cancellationToken);
            _last = summary;
        }
    }
}
