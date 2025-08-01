using Validation.Domain.Entities;
using Validation.Domain.Repositories;

namespace Validation.Tests;

public class InMemorySummaryRecordRepository : ISummaryRecordRepository
{
    public List<SummaryRecord> Records { get; } = new();

    public Task AddAsync(SummaryRecord record, CancellationToken ct = default)
    {
        Records.Add(record);
        return Task.CompletedTask;
    }

    public Task<SummaryRecord?> GetLatestAsync(string entity, CancellationToken ct = default)
    {
        var rec = Records.Where(r => r.Entity == entity)
            .OrderByDescending(r => r.RecordedAt)
            .FirstOrDefault();
        return Task.FromResult<SummaryRecord?>(rec);
    }
}
