using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Validation.Domain.Validation;
using Validation.Infrastructure.Metrics;

namespace Validation.Infrastructure;

public class PipelineOrchestrator
{
    private readonly IEnumerable<IMetricGatherer> _gatherers;
    private readonly ISummarisationService _summariser;
    private readonly IEnumerable<IValidationRule> _rules;
    private readonly SummarisationValidator _validator;
    private readonly IMetricsCollector _metrics;
    private readonly ILogger<PipelineOrchestrator> _logger;

    public PipelineOrchestrator(
        IEnumerable<IMetricGatherer> gatherers,
        ISummarisationService summariser,
        IEnumerable<IValidationRule> rules,
        SummarisationValidator validator,
        IMetricsCollector metrics,
        ILogger<PipelineOrchestrator> logger)
    {
        _gatherers = gatherers;
        _summariser = summariser;
        _rules = rules;
        _validator = validator;
        _metrics = metrics;
        _logger = logger;
    }

    public async Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        var data = await GatherDataAsync(cancellationToken);
        var summary = SummariseData(data);
        var valid = Validate(summary);
        await CommitAsync(summary, valid, cancellationToken);
    }

    public async Task<IEnumerable<decimal>> GatherDataAsync(CancellationToken ct = default)
    {
        var results = new List<decimal>();
        foreach (var g in _gatherers)
        {
            var vals = await g.GatherAsync(ct);
            results.AddRange(vals);
        }
        return results;
    }

    public decimal SummariseData(IEnumerable<decimal> values)
        => _summariser.Summarise(values);

    public bool Validate(decimal value)
        => _validator.Validate(0m, value, _rules);

    public Task CommitAsync(decimal summary, bool valid, CancellationToken ct = default)
    {
        _metrics.RecordValidationResult("pipeline", valid);
        _logger.LogInformation("Pipeline summary {Summary} valid {Valid}", summary, valid);
        return Task.CompletedTask;
    }
}
