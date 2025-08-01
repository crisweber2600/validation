namespace Validation.Infrastructure.Metrics.Pipeline;

public interface IMetricGatherer
{
    Task<IEnumerable<double>> GatherAsync(CancellationToken cancellationToken = default);
}
