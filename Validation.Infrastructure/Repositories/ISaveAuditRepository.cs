namespace Validation.Infrastructure.Repositories;

public interface ISaveAuditRepository : IRepository<SaveAudit>
{
    Task<SaveAudit?> GetLastAsync(Guid entityId, CancellationToken ct = default);
}
