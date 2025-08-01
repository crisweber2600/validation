using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Validation.Infrastructure.Metrics;

namespace Validation.Infrastructure.Pipeline;

public class PipelineOrchestrator
{
    private readonly IGatherService _gatherService;
    private readonly ISummarisationService _summarisationService;
    private readonly IValidationService _validationService;
    private readonly ICommitService _commitService;
    private readonly IMetricsCollector _metrics;
    private readonly ILogger<PipelineOrchestrator> _logger;

    public PipelineOrchestrator(
        IGatherService gatherService,
        ISummarisationService summarisationService,
        IValidationService validationService,
        ICommitService commitService,
        IMetricsCollector metrics,
        ILogger<PipelineOrchestrator> logger)
    {
        _gatherService = gatherService;
        _summarisationService = summarisationService;
        _validationService = validationService;
        _commitService = commitService;
        _metrics = metrics;
        _logger = logger;
    }

    public async Task ExecuteAsync<T>(CancellationToken cancellationToken = default)
    {
        var items = await _gatherService.GatherAsync<T>(cancellationToken);
        var sw = Stopwatch.StartNew();
        var summary = await _summarisationService.SummariseAsync(items, cancellationToken);
        var valid = await _validationService.ValidateAsync<T>(summary, cancellationToken);
        sw.Stop();

        var entityType = typeof(T).Name;
        _metrics.RecordValidationDuration(entityType, sw.Elapsed.TotalMilliseconds);
        _metrics.RecordValidationResult(entityType, valid);

        if (!valid)
        {
            _logger.LogWarning("Validation failed for {EntityType}; discarding.", entityType);
            return;
        }

        await _commitService.CommitAsync<T>(summary, cancellationToken);
    }
}
