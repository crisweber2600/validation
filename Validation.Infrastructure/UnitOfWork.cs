using Microsoft.EntityFrameworkCore;
using MassTransit;
using Validation.Infrastructure.Repositories;
using Validation.Domain.Validation;

namespace Validation.Infrastructure;

public class UnitOfWork
{
    private readonly DbContext _context;
    private readonly IValidationPlanProvider _planProvider;
    private readonly SummarisationValidator _validator;
    private readonly IPublishEndpoint _bus;
    private readonly Dictionary<Type, object> _repos = new();

    public UnitOfWork(DbContext context, IValidationPlanProvider planProvider, SummarisationValidator validator, IPublishEndpoint bus)
    {
        _context = context;
        _planProvider = planProvider;
        _validator = validator;
        _bus = bus;
    }

    public IGenericRepository<T> Repository<T>() where T : class
    {
        if (!_repos.TryGetValue(typeof(T), out var repo))
        {
            repo = new EfGenericRepository<T>(_context, _planProvider, _validator, _bus);
            _repos[typeof(T)] = repo;
        }
        return (IGenericRepository<T>)repo;
    }

    public async Task<int> SaveChangesWithPlanAsync<T>(CancellationToken ct = default) where T : class
    {
        await Repository<T>().SaveChangesWithPlanAsync(ct);
        return await _context.Set<T>().CountAsync(ct);
    }
}