namespace Validation.Domain.Repositories;

public interface IEntityRepository<T>
{
    Task SaveAsync(T entity, string? app = null, CancellationToken ct = default);
    Task DeleteAsync(Guid id, string? app = null, CancellationToken ct = default);
}
