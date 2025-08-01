namespace Validation.Infrastructure.Pipeline;

/// <summary>
/// Service that gathers raw metric values from a data source.
/// </summary>
public interface IGatherService
{
    /// <summary>
    /// Gather a collection of metric values.
    /// </summary>
    Task<IEnumerable<decimal>> GatherAsync(CancellationToken ct);
}
