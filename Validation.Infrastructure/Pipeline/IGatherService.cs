namespace Validation.Infrastructure.Pipeline;

/// <summary>
/// Service responsible for gathering raw metric values from any source.
/// </summary>
public interface IGatherService
{
    /// <summary>
    /// Retrieve a collection of metric values.
    /// </summary>
    Task<IEnumerable<decimal>> GatherAsync(CancellationToken ct);
}
