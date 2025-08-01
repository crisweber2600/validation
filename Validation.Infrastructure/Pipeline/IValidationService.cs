namespace Validation.Infrastructure.Pipeline;

/// <summary>
/// Validates a computed metric summary before it is committed.
/// </summary>
public interface IValidationService
{
    /// <summary>
    /// Returns true if the summary value is valid.
    /// </summary>
    Task<bool> ValidateAsync(decimal summary, CancellationToken ct);
}
