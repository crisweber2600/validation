using Microsoft.EntityFrameworkCore;
using Validation.Infrastructure.Repositories;

namespace Validation.Infrastructure.Repositories;

public class EfCoreSaveAuditRepository : ISaveAuditRepository
{
    private readonly DbContext _context;
    private readonly DbSet<SaveAudit> _set;

    public EfCoreSaveAuditRepository(DbContext context)
    {
        _context = context;
        _set = context.Set<SaveAudit>();
    }

    public async Task AddAsync(SaveAudit entity, CancellationToken ct = default)
    {
        await _set.AddAsync(entity, ct);
        await _context.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await _set.FindAsync(new object?[] { id }, ct);
        if (entity != null)
        {
            _set.Remove(entity);
            await _context.SaveChangesAsync(ct);
        }
    }

    public Task SoftDeleteAsync(Guid id, CancellationToken ct = default)
        => DeleteAsync(id, ct);

    public Task HardDeleteAsync(Guid id, CancellationToken ct = default)
        => DeleteAsync(id, ct);

    public async Task<SaveAudit?> GetAsync(Guid id, CancellationToken ct = default)
    {
        return await _set.FindAsync(new object?[] { id }, ct);
    }

    public async Task UpdateAsync(SaveAudit entity, CancellationToken ct = default)
    {
        _set.Update(entity);
        await _context.SaveChangesAsync(ct);
    }

    public async Task<SaveAudit?> GetLastAsync(Guid entityId, CancellationToken ct = default)
    {
        return await _set
            .Where(a => a.EntityId == entityId)
            .OrderByDescending(a => a.Timestamp)
            .FirstOrDefaultAsync(ct);
    }
}
