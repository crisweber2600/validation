using System.Collections.Generic;
namespace Validation.Infrastructure.Repositories;

public interface IGenericRepository<T> : IRepository<T>
{
    Task AddManyAsync(IEnumerable<T> items, CancellationToken ct = default);
    Task SaveChangesWithPlanAsync(CancellationToken ct = default);
}