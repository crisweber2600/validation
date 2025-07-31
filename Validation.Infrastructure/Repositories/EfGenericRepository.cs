using Microsoft.EntityFrameworkCore;
using Validation.Domain.Validation;

namespace Validation.Infrastructure.Repositories;

public class EfGenericRepository<T> : IGenericRepository<T> where T : class
{
    private readonly DbContext _context;
    private readonly DbSet<T> _set;
    private readonly List<T> _pending = new();

    public EfGenericRepository(DbContext context)
    {
        _context = context;
        _set = context.Set<T>();
    }

    public Task AddAsync(T entity, CancellationToken ct = default)
    {
        _pending.Add(entity);
        return Task.CompletedTask;
    }

    public Task AddManyAsync(IEnumerable<T> items, CancellationToken ct = default)
    {
        _pending.AddRange(items);
        return Task.CompletedTask;
    }

    public async Task SaveChangesWithPlanAsync(IValidationPlanProvider planProvider, SummarisationValidator validator, CancellationToken ct = default)
    {
        var rules = planProvider.GetRules<T>();
        if (_pending.Count > 0)
        {
            validator.Validate(0, 0, rules);
            await _set.AddRangeAsync(_pending, ct);
            await _context.SaveChangesAsync(ct);
            _pending.Clear();
        }
    }
}
