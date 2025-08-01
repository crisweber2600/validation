using Validation.Domain.Entities;

namespace Validation.Infrastructure.Repositories;

public interface ISummaryRecordRepository : IRepository<SummaryRecord>
{
    Task<SummaryRecord?> GetLatestAsync(string programName, string entity, CancellationToken ct = default);
}
