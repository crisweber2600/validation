namespace Validation.Infrastructure;

public interface IMetricGatherer
{
    Task<IEnumerable<decimal>> GatherAsync(CancellationToken cancellationToken = default);
}
