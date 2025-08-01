using Validation.Domain.Validation;

namespace Validation.Infrastructure.Pipeline;

/// <summary>
/// Validates a computed metric against the configured plan and the last persisted value.
/// </summary>
public interface IValidationService
{
    /// <summary>
    /// Validate a metric for the given entity type.
    /// </summary>
    Task<bool> ValidateAsync<T>(Guid entityId, decimal metric, CancellationToken ct = default);
}
