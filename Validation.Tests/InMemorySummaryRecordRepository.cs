using Validation.Domain.Entities;
using Validation.Infrastructure.Repositories;

namespace Validation.Tests;

public class InMemorySummaryRecordRepository : ISummaryRecordRepository
{
    public List<SummaryRecord> Records { get; } = new();

    public Task AddAsync(SummaryRecord record, CancellationToken ct = default)
    {
        Records.Add(record);
        return Task.CompletedTask;
    }

    public Task<SummaryRecord?> GetLatestAsync(string programName, string entity, CancellationToken ct = default)
    {
        var result = Records
            .Where(r => r.ProgramName == programName && r.Entity == entity)
            .OrderByDescending(r => r.RecordedAt)
            .FirstOrDefault();
        return Task.FromResult<SummaryRecord?>(result);
    }
}
