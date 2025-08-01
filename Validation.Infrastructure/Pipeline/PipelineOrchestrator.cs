using System.Diagnostics;
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

    public event Action<double>? Discarded;

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

    public async Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        var sw = Stopwatch.StartNew();
        var data = await _gather.GatherAsync(cancellationToken);
        var summary = await _summarise.SummariseAsync(data, cancellationToken);
        var valid = _validate.Validate(summary);
        sw.Stop();

        _metrics.RecordValidationDuration("pipeline", sw.Elapsed.TotalMilliseconds);
        _metrics.RecordValidationResult("pipeline", valid);

        if (!valid)
        {
            Discarded?.Invoke(summary);
            _logger.LogInformation("Pipeline discarded value {Value}", summary);
            return;
        }

        await _commit.CommitAsync(summary, cancellationToken);
    }
}
