namespace Validation.Infrastructure.Metrics;

public interface IMetricsGatherer
{
    Task<IEnumerable<decimal>> GatherAsync(CancellationToken cancellationToken = default);
}

public interface ISummarisationService
{
    Task<decimal> SummariseAsync(IEnumerable<decimal> values, CancellationToken ct = default);
}

public class MetricsPipelineOptions
{
    public int IntervalMs { get; set; } = 60000;
    public Validation.Domain.Validation.ValidationPlan ValidationPlan { get; set; } = new(new Validation.Domain.Validation.IValidationRule[] { });
}
