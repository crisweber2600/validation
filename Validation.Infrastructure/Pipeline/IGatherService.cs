namespace Validation.Infrastructure.Pipeline;

/// <summary>
/// Retrieves raw metric values from an external source.
/// </summary>
public interface IGatherService
{
    /// <summary>
    /// Gather the next set of metrics.
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    Task<IEnumerable<decimal>> GatherAsync(CancellationToken ct);
}
