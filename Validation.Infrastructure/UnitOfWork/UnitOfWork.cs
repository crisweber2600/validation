using Microsoft.EntityFrameworkCore;
using Validation.Domain.Validation;
using Validation.Infrastructure.Repositories;

namespace Validation.Infrastructure.UnitOfWork;

public class UnitOfWork<TContext> : IUnitOfWork where TContext : DbContext
{
    private readonly TContext _context;
    private readonly SummarisationValidator _validator;
    private readonly IValidationPlanProvider _planProvider;

    public UnitOfWork(TContext context, SummarisationValidator validator, IValidationPlanProvider planProvider)
    {
        _context = context;
        _validator = validator;
        _planProvider = planProvider;
    }

    public IRepository<T> Repository<T>() where T : class => new EfCoreRepository<T>(_context);

    public Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        return _context.SaveChangesAsync(ct);
    }

    public async Task<int> SaveChangesWithPlanAsync<T>(CancellationToken ct = default) where T : class
    {
        foreach (var entry in _context.ChangeTracker.Entries<T>())
        {
            if (entry.State == EntityState.Modified || entry.State == EntityState.Added)
            {
                var rules = _planProvider.GetRules<T>();
                decimal previous = 0m;
                decimal current = 0m;
                if (entry.Properties.Any(p => p.Metadata.Name == "Metric"))
                {
                    if (entry.State == EntityState.Modified)
                        previous = entry.OriginalValues.GetValue<decimal>("Metric");
                    current = entry.CurrentValues.GetValue<decimal>("Metric");
                }

                if (!_validator.Validate(previous, current, rules))
                {
                    throw new InvalidOperationException("Validation failed");
                }
            }
        }

        return await _context.SaveChangesAsync(ct);
    }
}
