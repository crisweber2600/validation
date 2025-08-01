using Microsoft.EntityFrameworkCore;
using Validation.Domain;

namespace Validation.Infrastructure.Repositories;

public class EfNannyRecordRepository : INannyRecordRepository
{
    private readonly DbContext _context;
    private readonly DbSet<NannyRecord> _set;

    public EfNannyRecordRepository(DbContext context)
    {
        _context = context;
        _set = context.Set<NannyRecord>();
    }

    public async Task AddAsync(NannyRecord entity, CancellationToken ct = default)
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

    public async Task<NannyRecord?> GetAsync(Guid id, CancellationToken ct = default)
    {
        return await _set.FindAsync(new object?[] { id }, ct);
    }

    public async Task UpdateAsync(NannyRecord entity, CancellationToken ct = default)
    {
        _set.Update(entity);
        await _context.SaveChangesAsync(ct);
    }

    public async Task<NannyRecord?> GetLastAsync(Guid entityId, CancellationToken ct = default)
    {
        return await _set
            .Where(x => x.EntityId == entityId)
            .OrderByDescending(x => x.Timestamp)
            .FirstOrDefaultAsync(ct);
    }
}
