using Microsoft.EntityFrameworkCore;
using Validation.Domain.Entities;

namespace Validation.Infrastructure.Repositories;

public class EfCoreSummaryRecordRepository : ISummaryRecordRepository
{
    private readonly DbContext _context;
    private readonly DbSet<SummaryRecord> _set;

    public EfCoreSummaryRecordRepository(DbContext context)
    {
        _context = context;
        _set = context.Set<SummaryRecord>();
    }

    public async Task AddAsync(SummaryRecord entity, CancellationToken ct = default)
    {
        await _set.AddAsync(entity, ct);
        await _context.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var record = await _set.FindAsync(new object?[] { id }, ct);
        if (record != null)
        {
            _set.Remove(record);
            await _context.SaveChangesAsync(ct);
        }
    }

    public async Task<SummaryRecord?> GetAsync(Guid id, CancellationToken ct = default)
    {
        return await _set.FindAsync(new object?[] { id }, ct);
    }

    public async Task UpdateAsync(SummaryRecord entity, CancellationToken ct = default)
    {
        _set.Update(entity);
        await _context.SaveChangesAsync(ct);
    }

    public async Task<SummaryRecord?> GetLatestAsync(string programName, string entity, CancellationToken ct = default)
    {
        return await _set
            .Where(r => r.ProgramName == programName && r.Entity == entity)
            .OrderByDescending(r => r.RecordedAt)
            .FirstOrDefaultAsync(ct);
    }
}
