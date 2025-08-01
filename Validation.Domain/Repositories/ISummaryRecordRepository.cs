using Validation.Domain.Entities;

namespace Validation.Domain.Repositories;

public interface ISummaryRecordRepository
{
    Task AddAsync(SummaryRecord record, CancellationToken ct = default);
    Task<decimal?> GetLatestValueAsync(string programName, string entity, CancellationToken ct = default);
}
