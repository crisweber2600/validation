namespace Validation.Infrastructure.Repositories;

public interface ISaveAuditRepository : IRepository<SaveAudit>
{
    Task<SaveAudit?> GetLastAsync(string entityKey, CancellationToken ct = default);
    new Task AddAsync(SaveAudit audit, CancellationToken ct = default);   // NEW
}
