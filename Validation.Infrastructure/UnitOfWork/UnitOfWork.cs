using Microsoft.EntityFrameworkCore;
using Validation.Domain.Validation;
using Validation.Infrastructure;
using Validation.Infrastructure.Repositories;
namespace Validation.Infrastructure.UnitOfWork;


public class UnitOfWork<TContext> : IUnitOfWork where TContext : DbContext
{
    private readonly TContext _context;
    private readonly SummarisationValidator _validator;
    private readonly IValidationPlanProvider _planProvider;
    private readonly Dictionary<Type, object> _repos = new();

    public UnitOfWork(TContext context, SummarisationValidator validator, IValidationPlanProvider planProvider)
    {
        _context = context;
        _validator = validator;
        _planProvider = planProvider;
    }

    public IRepository<T> Repository<T>() where T : class
    {
        if (!_repos.TryGetValue(typeof(T), out var repo))
        {
            repo = new EfCoreRepository<T>(_context);
            _repos[typeof(T)] = repo;
        }
        return (IRepository<T>)repo;
    }

    public Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        return _context.SaveChangesAsync(ct);
    }

    public async Task<int> SaveChangesWithPlanAsync<T>(CancellationToken ct = default)
    {
        var rules = _planProvider.GetRules<T>();
        foreach (var entry in _context.ChangeTracker.Entries<SaveAudit>().Where(e => e.State == EntityState.Added))
        {
            var audit = entry.Entity;
            var last = await _context.Set<SaveAudit>()
                .Where(a => a.EntityId == audit.EntityId && a.Id != audit.Id)
                .OrderByDescending(a => a.Timestamp)
                .FirstOrDefaultAsync(ct);
            audit.IsValid = _validator.Validate(last?.Metric ?? 0m, audit.Metric, rules);
        }
        return await _context.SaveChangesAsync(ct);
    }
}
