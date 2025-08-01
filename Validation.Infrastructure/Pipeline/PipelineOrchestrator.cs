using Microsoft.Extensions.Logging;
using Validation.Infrastructure.Metrics;

namespace Validation.Infrastructure.Pipeline;

public class PipelineOrchestrator
{
    private readonly IGatherService _gather;
    private readonly ISummarisationService _summarise;
    private readonly IValidationService _validate;
    private readonly ICommitService _commit;
    private readonly IMetricsCollector _metrics;
    private readonly ILogger<PipelineOrchestrator> _logger;

    public PipelineOrchestrator(
        IGatherService gather,
        ISummarisationService summarise,
        IValidationService validate,
        ICommitService commit,
        IMetricsCollector metrics,
        ILogger<PipelineOrchestrator> logger)
    {
        _gather = gather;
        _summarise = summarise;
        _validate = validate;
        _commit = commit;
        _metrics = metrics;
        _logger = logger;
    }

    public event Action<Type>? Discarded;

    public async Task ExecuteAsync<T>(CancellationToken cancellationToken = default)
    {
        var gathered = await _gather.GatherAsync<T>(cancellationToken);
        var summary = await _summarise.SummariseAsync(gathered, cancellationToken);
        var valid = await _validate.ValidateAsync(summary, cancellationToken);

        _metrics.RecordValidationResult(typeof(T).Name, valid);

        if (!valid)
        {
            Discarded?.Invoke(typeof(T));
            return;
        }

        await _commit.CommitAsync(summary, cancellationToken);
    }
}
