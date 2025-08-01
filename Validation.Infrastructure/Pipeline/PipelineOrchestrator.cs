using Microsoft.Extensions.Hosting;

namespace Validation.Infrastructure.Pipeline;

public class PipelineOrchestrator<T> : IHostedService
{
    private readonly IGatherService _gather;
    private readonly SummarizationService _summary;
    private readonly ValidationService _validation;
    private readonly CommitService _commit;
    private readonly DiscardHandler _discard;

    public PipelineOrchestrator(IGatherService gather, SummarizationService summary, ValidationService validation, CommitService commit, DiscardHandler discard)
    {
        _gather = gather;
        _summary = summary;
        _validation = validation;
        _commit = commit;
        _discard = discard;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await ExecuteAsync(cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public async Task ExecuteAsync(CancellationToken ct)
    {
        var metrics = await _gather.GatherAsync(ct);
        var summary = _summary.Summarize(metrics);
        var valid = await _validation.ValidateAsync<T>(summary, ct);
        if (valid)
            await _commit.CommitAsync<T>(summary, true, ct);
        else
            await _discard.HandleAsync<T>(summary, ct);
    }
}
