using Microsoft.EntityFrameworkCore;
using Validation.Domain.Validation;

namespace Validation.Infrastructure.Repositories;

public interface IUnitOfWork : IDisposable
{
    IGenericRepository<TEntity> Repository<TEntity>() where TEntity : class;
    Task SaveChangesWithPlanAsync<TEntity>(CancellationToken ct = default) where TEntity : class;
}

public class EfUnitOfWork : IUnitOfWork
{
    private readonly DbContext _context;
    private readonly IValidationPlanProvider _planProvider;
    private readonly SummarisationValidator _validator;

    public EfUnitOfWork(DbContext context, IValidationPlanProvider planProvider, SummarisationValidator validator)
    {
        _context = context;
        _planProvider = planProvider;
        _validator = validator;
    }

    public IGenericRepository<TEntity> Repository<TEntity>() where TEntity : class
        => new EfGenericRepository<TEntity>(_context, _planProvider, _validator);

    public async Task SaveChangesWithPlanAsync<TEntity>(CancellationToken ct = default) where TEntity : class
    {
        var repo = new EfGenericRepository<TEntity>(_context, _planProvider, _validator);
        await repo.SaveChangesWithPlanAsync(ct);
    }

    public void Dispose() => _context.Dispose();
}
