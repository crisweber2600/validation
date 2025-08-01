using Validation.Domain.Entities;

namespace Validation.Domain.Repositories;

public interface ISummaryRecordRepository
{
    Task AddAsync(SummaryRecord record, CancellationToken ct = default);
    Task<SummaryRecord?> GetLatestAsync(string entity, CancellationToken ct = default);
}
