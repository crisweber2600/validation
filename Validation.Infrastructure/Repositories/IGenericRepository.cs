namespace Validation.Infrastructure.Repositories;

using Validation.Domain.Validation;

public interface IGenericRepository<T>
{
    Task AddAsync(T entity, CancellationToken ct = default);
    Task AddManyAsync(IEnumerable<T> items, CancellationToken ct = default);
    Task SaveChangesWithPlanAsync(IValidationPlanProvider planProvider, SummarisationValidator validator, CancellationToken ct = default);
}
