using Microsoft.EntityFrameworkCore;
using Validation.Domain.Validation;

namespace Validation.Infrastructure.Repositories;

public class EfGenericRepository<T> : IGenericRepository<T> where T : class
{
    private readonly DbContext _context;
    private readonly DbSet<T> _set;
    private readonly ISummarisationValidator _validator;
    private readonly IValidationPlanProvider _planProvider;

    public EfGenericRepository(DbContext context, ISummarisationValidator validator, IValidationPlanProvider planProvider)
    {
        _context = context;
        _set = context.Set<T>();
        _validator = validator;
        _planProvider = planProvider;
    }

    public Task AddAsync(T entity, CancellationToken ct = default)
    {
        _set.Add(entity);
        return Task.CompletedTask;
    }

    public Task AddManyAsync(IEnumerable<T> items, CancellationToken ct = default)
    {
        _set.AddRange(items);
        return Task.CompletedTask;
    }

    public async Task<T?> GetAsync(Guid id, CancellationToken ct = default)
    {
        return await _set.FindAsync(new object?[] { id }, ct);
    }

    public Task UpdateAsync(T entity, CancellationToken ct = default)
    {
        _set.Update(entity);
        return Task.CompletedTask;
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await _set.FindAsync(new object?[] { id }, ct);
        if (entity != null)
            _set.Remove(entity);
    }

    public async Task SaveChangesWithPlanAsync(CancellationToken ct = default)
    {
        _validator.Validate(0, 0, _planProvider.GetRules<T>());
        await _context.SaveChangesAsync(ct);
    }
}
