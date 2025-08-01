using Microsoft.Extensions.Hosting;

namespace Validation.Infrastructure.Pipeline;

/// <summary>
/// Hosted service that runs the metric pipeline from gather through validation and commit.
/// </summary>
public class PipelineOrchestrator<T> : IHostedService
{
    private readonly IGatherService _gather;
    private readonly SummarizationService _summarizer;
    private readonly IValidationService _validator;
    private readonly CommitService _committer;
    private readonly DiscardHandler _discard;
    private readonly Guid _entityId = Guid.NewGuid();

    public PipelineOrchestrator(
        IGatherService gather,
        SummarizationService summarizer,
        IValidationService validator,
        CommitService committer,
        DiscardHandler discard)
    {
        _gather = gather;
        _summarizer = summarizer;
        _validator = validator;
        _committer = committer;
        _discard = discard;
    }

    /// <summary>
    /// Execute the pipeline once for type <typeparamref name="T"/>.
    /// </summary>
    public async Task ExecuteAsync(CancellationToken ct = default)
    {
        var metrics = await _gather.GatherAsync(ct);
        var summary = _summarizer.Summarize(metrics);
        var valid = await _validator.ValidateAsync<T>(_entityId, summary, ct);
        if (valid)
            await _committer.CommitAsync<T>(_entityId, summary, ct);
        else
            await _discard.HandleAsync<T>(_entityId, summary, ct);
    }

    public Task StartAsync(CancellationToken cancellationToken) => ExecuteAsync(cancellationToken);

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
