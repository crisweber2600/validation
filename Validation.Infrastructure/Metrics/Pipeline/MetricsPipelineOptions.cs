namespace Validation.Infrastructure.Metrics.Pipeline;

public class MetricsPipelineOptions
{
    public TimeSpan RunInterval { get; set; } = TimeSpan.FromSeconds(30);
}
