namespace Validation.Infrastructure.Pipeline;

public interface IGatherService
{
    Task<IEnumerable<double>> GatherAsync(CancellationToken cancellationToken = default);
}
