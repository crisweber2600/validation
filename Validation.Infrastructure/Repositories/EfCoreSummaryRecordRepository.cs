using Microsoft.EntityFrameworkCore;
using Validation.Domain.Entities;
using Validation.Domain.Repositories;

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

    public async Task AddAsync(SummaryRecord record, CancellationToken ct = default)
    {
        await _set.AddAsync(record, ct);
        await _context.SaveChangesAsync(ct);
    }

    public async Task<decimal?> GetLatestValueAsync(string programName, string entity, CancellationToken ct = default)
    {
        return await _set
            .Where(r => r.ProgramName == programName && r.Entity == entity)
            .OrderByDescending(r => r.RecordedAt)
            .Select(r => (decimal?)r.MetricValue)
            .FirstOrDefaultAsync(ct);
    }
}
