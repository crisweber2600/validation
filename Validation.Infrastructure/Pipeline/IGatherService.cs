namespace Validation.Infrastructure.Pipeline;

public interface IGatherService
{
    Task<IEnumerable<T>> GatherAsync<T>(CancellationToken cancellationToken = default);
}
