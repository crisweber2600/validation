using Validation.Domain;

namespace Validation.Infrastructure.Repositories;

public interface INannyRecordRepository : IRepository<NannyRecord>
{
    Task<NannyRecord?> GetLastAsync(Guid entityId, CancellationToken ct = default);
}
