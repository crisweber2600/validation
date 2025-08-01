namespace Validation.Domain.Repositories;

using System.Threading;
using global::Validation.Domain.Entities;

public interface ISummaryRecordRepository
{
    Task AddAsync(SummaryRecord record, CancellationToken ct = default);
    Task<decimal?> GetLatestValueAsync(string programName, string entity, CancellationToken ct = default);
}
