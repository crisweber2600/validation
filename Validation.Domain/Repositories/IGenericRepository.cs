namespace Validation.Domain.Repositories;

public interface IGenericRepository<T>
{
    Task AddAsync(T item, CancellationToken ct = default);
    Task AddManyAsync(IEnumerable<T> items, CancellationToken ct = default);
    Task SaveChangesWithPlanAsync(CancellationToken ct = default);
}
