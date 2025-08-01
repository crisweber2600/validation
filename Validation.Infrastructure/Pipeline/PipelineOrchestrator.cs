using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Validation.Domain.Validation;

namespace Validation.Infrastructure.Pipeline;

/// <summary>
/// Coordinates each step of the metrics pipeline from gathering through committing or discarding.
/// </summary>
public class PipelineOrchestrator<T> : IHostedService
{
    private readonly IGatherService _gather;
    private readonly SummarizationService _summarizer;
    private readonly IValidationService _validator;
    private readonly CommitService _commit;
    private readonly DiscardHandler _discard;

    public PipelineOrchestrator(IGatherService gather, SummarizationService summarizer, IValidationService validator, CommitService commit, DiscardHandler discard)
    {
        _gather = gather;
        _summarizer = summarizer;
        _validator = validator;
        _commit = commit;
        _discard = discard;
    }

    /// <summary>
    /// Runs the full gather-&gt;summarize-&gt;validate flow for <typeparamref name="T"/>.
    /// </summary>
    public async Task ExecuteAsync(CancellationToken ct)
    {
        var metrics = await _gather.GatherAsync(ct);
        var summary = await _summarizer.SummarizeAsync(metrics, ValidationStrategy.Sum, ct);
        var valid = await _validator.ValidateAsync(summary, ct);

        if (valid)
            await _commit.CommitAsync(summary, valid, ct);
        else
            await _discard.HandleAsync(summary, ct);
    }

    public Task StartAsync(CancellationToken cancellationToken) => ExecuteAsync(cancellationToken);

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
