namespace Validation.Infrastructure.Pipeline;

public interface IGatherService
{
    Task<IEnumerable<decimal>> GatherAsync(CancellationToken ct);
}
