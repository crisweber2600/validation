using Microsoft.EntityFrameworkCore;
using Validation.Domain.Repositories;
using Validation.Domain.Validation;

namespace Validation.Infrastructure.Repositories;

public class EfGenericRepository<T> : IGenericRepository<T> where T : class
{
    private readonly DbContext _context;
    private readonly DbSet<T> _set;
    private readonly ISummarisationValidator _validator;
    private readonly IValidationPlanProvider _planProvider;
    private readonly List<T> _pending = new();

    public EfGenericRepository(DbContext context, ISummarisationValidator validator, IValidationPlanProvider planProvider)
    {
        _context = context;
        _set = context.Set<T>();
        _validator = validator;
        _planProvider = planProvider;
    }

    public Task AddAsync(T item, CancellationToken ct = default)
    {
        _pending.Add(item);
        return Task.CompletedTask;
    }

    public Task AddManyAsync(IEnumerable<T> items, CancellationToken ct = default)
    {
        _pending.AddRange(items);
        return Task.CompletedTask;
    }

    public async Task SaveChangesWithPlanAsync(CancellationToken ct = default)
    {
        var rules = _planProvider.GetRules<T>();
        _validator.Validate(0, 0, rules);

        if (_pending.Count > 0)
        {
            await _set.AddRangeAsync(_pending, ct);
            _pending.Clear();
            await _context.SaveChangesAsync(ct);
        }
    }
}
