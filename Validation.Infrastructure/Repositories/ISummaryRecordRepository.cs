using Validation.Domain.Entities;

namespace Validation.Infrastructure.Repositories;

public interface ISummaryRecordRepository
{
    Task AddAsync(SummaryRecord record, CancellationToken ct = default);
    Task<SummaryRecord?> GetLatestAsync(string programName, string entity, CancellationToken ct = default);
}
