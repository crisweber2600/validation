using Validation.Infrastructure;
using Validation.Infrastructure.Repositories;

namespace Validation.Tests;

public class InMemorySaveAuditRepository : ISaveAuditRepository
{
    public List<SaveAudit> Audits { get; } = new();

    public Task AddAsync(SaveAudit entity, CancellationToken ct = default)
    {
        Audits.Add(entity);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        Audits.RemoveAll(a => a.Id == id);
        return Task.CompletedTask;
    }

    public Task<SaveAudit?> GetAsync(Guid id, CancellationToken ct = default)
    {
        return Task.FromResult<SaveAudit?>(Audits.FirstOrDefault(a => a.Id == id));
    }

    public Task<SaveAudit?> GetLastAsync(Guid entityId, CancellationToken ct = default)
    {
        var audit = Audits.Where(a => a.EntityId == entityId)
            .OrderByDescending(a => a.Timestamp)
            .FirstOrDefault();
        return Task.FromResult<SaveAudit?>(audit);
    }

    public Task UpdateAsync(SaveAudit entity, CancellationToken ct = default)
    {
        var index = Audits.FindIndex(a => a.Id == entity.Id);
        if (index >= 0) Audits[index] = entity;
        return Task.CompletedTask;
    }
}
