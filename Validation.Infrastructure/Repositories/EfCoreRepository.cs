using Microsoft.EntityFrameworkCore;

namespace Validation.Infrastructure.Repositories;

public class EfCoreRepository<T> : IRepository<T> where T : class
{
    private readonly DbContext _context;
    private readonly DbSet<T> _set;

    public EfCoreRepository(DbContext context)
    {
        _context = context;
        _set = context.Set<T>();
    }

    public async Task<T?> GetAsync(Guid id, CancellationToken ct = default)
    {
        return await _set.FindAsync(new object?[] { id }, ct);
    }

    public async Task AddAsync(T entity, CancellationToken ct = default)
    {
        await _set.AddAsync(entity, ct);
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
}
