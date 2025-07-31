namespace Validation.Infrastructure.Repositories;

public interface IGenericRepository<T>
{
    Task<T?> GetAsync(Guid id, CancellationToken ct = default);
    Task AddAsync(T entity, CancellationToken ct = default);
    Task AddManyAsync(IEnumerable<T> items, CancellationToken ct = default);
    Task UpdateAsync(T entity, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
    Task SaveChangesWithPlanAsync(CancellationToken ct = default);
}
