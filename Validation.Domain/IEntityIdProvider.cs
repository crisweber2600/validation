namespace Validation.Domain;

public interface IEntityIdProvider
{
    /// <summary>
    /// Returns a stable discriminator key for audit/sequence validation.
    /// Implementations must never return null or empty.
    /// </summary>
    string GetId<T>(T entity);
}
