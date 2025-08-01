using Microsoft.Extensions.Logging;

namespace Validation.Infrastructure.Pipeline;

public class PipelineOrchestrator
{
    private readonly IGatherService _gather;
    private readonly ISummarisationService _summarise;
    private readonly IValidationService _validate;
    private readonly ICommitService _commit;
    private readonly ILogger<PipelineOrchestrator> _logger;

    public event EventHandler<PipelineDiscardedEvent>? Discarded;

    public PipelineOrchestrator(
        IGatherService gather,
        ISummarisationService summarise,
        IValidationService validate,
        ICommitService commit,
        ILogger<PipelineOrchestrator>? logger = null)
    {
        _gather = gather;
        _summarise = summarise;
        _validate = validate;
        _commit = commit;
        _logger = logger ?? Microsoft.Extensions.Logging.Abstractions.NullLogger<PipelineOrchestrator>.Instance;
    }

    public async Task ExecuteAsync<T>(CancellationToken ct = default)
    {
        var data = await _gather.GatherAsync<T>(ct);
        var summary = await _summarise.SummariseAsync(data, ct);
        var valid = await _validate.ValidateAsync(summary, ct);

        if (valid)
        {
            await _commit.CommitAsync(summary, ct);
            _logger.LogInformation("Committed summary for {Type}", typeof(T).Name);
        }
        else
        {
            _logger.LogInformation("Discarding summary for {Type}", typeof(T).Name);
            Discarded?.Invoke(this, new PipelineDiscardedEvent(typeof(T), summary));
        }
    }
}
