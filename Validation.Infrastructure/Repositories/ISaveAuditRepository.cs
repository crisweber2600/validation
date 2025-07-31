namespace Validation.Infrastructure.Repositories;

public interface ISaveAuditRepository : IRepository<SaveAudit>
{
    Task<SaveAudit?> GetLastForEntityAsync(Guid entityId, CancellationToken ct = default);
}
