using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Validation.Domain.Validation;
using Validation.Infrastructure.Repositories;
using Validation.Infrastructure;

namespace Validation.Infrastructure.UnitOfWork;

public class UnitOfWork<TContext> : IUnitOfWork where TContext : DbContext
{
    private readonly TContext _context;
    private readonly IValidationPlanProvider _planProvider;
    private readonly SummarisationValidator _validator;

    public UnitOfWork(TContext context, IValidationPlanProvider planProvider, SummarisationValidator validator)
    {
        _context = context;
        _planProvider = planProvider;
        _validator = validator;
    }

    public IRepository<T> Repository<T>() where T : class
    {
        return new EfCoreRepository<T>(_context);
    }

    public Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        return _context.SaveChangesAsync(ct);
    }

    public async Task<int> SaveChangesWithPlanAsync<T>(CancellationToken ct = default)
    {
        var rules = _planProvider.GetRules<T>();
        var audits = _context.ChangeTracker.Entries<SaveAudit>()
            .Where(e => e.State == EntityState.Added)
            .Select(e => e.Entity)
            .ToList();

        foreach (var audit in audits)
        {
            var last = await _context.Set<SaveAudit>()
                .Where(a => a.EntityId == audit.EntityId && a.Id != audit.Id)
                .OrderByDescending(a => a.Timestamp)
                .FirstOrDefaultAsync(ct);
            audit.IsValid = _validator.Validate(last?.Metric ?? 0m, audit.Metric, rules);
        }

        return await _context.SaveChangesAsync(ct);
    }
}
