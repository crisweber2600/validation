using Microsoft.Extensions.Hosting;

namespace Validation.Infrastructure.Pipeline;

/// <summary>
/// Coordinates the metric processing pipeline from gathering through commit or discard.
/// </summary>
public class PipelineOrchestrator<T> : IHostedService
{
    private readonly IGatherService _gather;
    private readonly SummarizationService _summarizer;
    private readonly ValidationService _validator;
    private readonly CommitService _commit;
    private readonly DiscardHandler _discard;

    public PipelineOrchestrator(
        IGatherService gather,
        SummarizationService summarizer,
        ValidationService validator,
        CommitService commit,
        DiscardHandler discard)
    {
        _gather = gather;
        _summarizer = summarizer;
        _validator = validator;
        _commit = commit;
        _discard = discard;
    }

    /// <summary>
    /// Execute the pipeline once.
    /// </summary>
    public async Task ExecuteAsync(CancellationToken ct)
    {
        var metrics = await _gather.GatherAsync(ct);
        var summary = await _summarizer.SummarizeAsync(metrics, ct);
        var entityId = Guid.NewGuid();
        var valid = await _validator.ValidateAsync<T>(entityId, summary, ct);
        if (valid)
        {
            await _commit.CommitAsync<T>(entityId, summary, valid, ct);
        }
        else
        {
            await _discard.HandleAsync<T>(metrics, ct);
        }
    }

    public Task StartAsync(CancellationToken cancellationToken) => ExecuteAsync(cancellationToken);

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
