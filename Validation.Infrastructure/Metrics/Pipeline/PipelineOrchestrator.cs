using Microsoft.Extensions.Logging;
using Validation.Domain.Validation;

namespace Validation.Infrastructure.Metrics;

public class PipelineOrchestrator
{
    private readonly IEnumerable<IMetricsGatherer> _gatherers;
    private readonly ISummarisationService _summariser;
    private readonly SummarisationValidator _validator;
    private readonly IMetricsCollector _collector;
    private readonly MetricsPipelineOptions _options;
    private readonly ILogger<PipelineOrchestrator> _logger;

    public PipelineOrchestrator(
        IEnumerable<IMetricsGatherer> gatherers,
        ISummarisationService summariser,
        SummarisationValidator validator,
        IMetricsCollector collector,
        MetricsPipelineOptions options,
        ILogger<PipelineOrchestrator> logger)
    {
        _gatherers = gatherers;
        _summariser = summariser;
        _validator = validator;
        _collector = collector;
        _options = options;
        _logger = logger;
    }

    public async Task RunAsync(CancellationToken cancellationToken = default)
    {
        var values = new List<decimal>();
        foreach (var g in _gatherers)
        {
            values.AddRange(await g.GatherAsync(cancellationToken));
        }

        var summary = await _summariser.SummariseAsync(values, cancellationToken);

        if (_validator.Validate(0m, summary, _options.ValidationPlan))
        {
            _collector.RecordValidationDuration("pipeline", (double)summary);
        }
        else
        {
            _logger.LogWarning("Metrics summary failed validation: {Summary}", summary);
        }
    }
}
