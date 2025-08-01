using Microsoft.EntityFrameworkCore;
using Validation.Domain.Entities;
using Validation.Domain.Validation;

namespace Validation.Infrastructure;

public class UnitOfWork
{
    private readonly DbContext _context;
    private readonly SummarisationValidator _validator;
    private decimal _previousMetric;

    public UnitOfWork(DbContext context, SummarisationValidator validator)
    {
        _context = context;
        _validator = validator;
    }

    public async Task SaveChangesAsync<T>(ValidationPlan ruleSet, CancellationToken ct = default)
        where T : class, IValidatableEntity
    {
        foreach (var entry in _context.ChangeTracker.Entries<T>())
        {
            var entity = entry.Entity;
            var isValid = _validator.Validate(_previousMetric, entity.Metric, ruleSet);
            entity.Validated = isValid;
            _previousMetric = entity.Metric;
        }

        await _context.SaveChangesAsync(ct);
    }
}
