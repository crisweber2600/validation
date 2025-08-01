namespace Validation.Infrastructure.Repositories;

public interface INannyRecordRepository : IRepository<Validation.Domain.NannyRecord>
{
    Task<Validation.Domain.NannyRecord?> GetLastAsync(Guid entityId, CancellationToken ct = default);
}
