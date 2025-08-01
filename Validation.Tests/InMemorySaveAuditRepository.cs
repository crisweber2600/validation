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
        var idString = id.ToString();
        Audits.RemoveAll(a => a.Id == idString);
        return Task.CompletedTask;
    }

    public Task<SaveAudit?> GetAsync(Guid id, CancellationToken ct = default)
    {
        var idString = id.ToString();
        return Task.FromResult<SaveAudit?>(Audits.FirstOrDefault(a => a.Id == idString));
    }

    public Task UpdateAsync(SaveAudit entity, CancellationToken ct = default)
    {
        var index = Audits.FindIndex(a => a.Id == entity.Id);
        if (index >= 0) Audits[index] = entity;
        return Task.CompletedTask;
    }

    public Task<SaveAudit?> GetLastAsync(string entityId, CancellationToken ct = default)
    {
        var audit = Audits.Where(a => a.EntityId == entityId)
            .OrderByDescending(a => a.Timestamp)
            .FirstOrDefault();
        return Task.FromResult<SaveAudit?>(audit);
    }

    public Task<SaveAudit?> GetLastAsync(Guid entityId, CancellationToken ct = default)
    {
        return GetLastAsync(entityId.ToString(), ct);
    }

    public Task<IEnumerable<SaveAudit>> GetByEntityTypeAsync(string entityType, CancellationToken ct = default)
    {
        var audits = Audits.Where(a => a.EntityType == entityType)
            .OrderByDescending(a => a.Timestamp);
        return Task.FromResult<IEnumerable<SaveAudit>>(audits);
    }

    public Task<IEnumerable<SaveAudit>> GetByApplicationAsync(string applicationName, CancellationToken ct = default)
    {
        var audits = Audits.Where(a => a.ApplicationName == applicationName)
            .OrderByDescending(a => a.Timestamp);
        return Task.FromResult<IEnumerable<SaveAudit>>(audits);
    }

    public Task<IEnumerable<SaveAudit>> GetByTimeRangeAsync(DateTime from, DateTime to, CancellationToken ct = default)
    {
        var audits = Audits.Where(a => a.Timestamp >= from && a.Timestamp <= to)
            .OrderByDescending(a => a.Timestamp);
        return Task.FromResult<IEnumerable<SaveAudit>>(audits);
    }

    public Task<IEnumerable<SaveAudit>> GetByCorrelationIdAsync(string correlationId, CancellationToken ct = default)
    {
        var audits = Audits.Where(a => a.CorrelationId == correlationId)
            .OrderByDescending(a => a.Timestamp);
        return Task.FromResult<IEnumerable<SaveAudit>>(audits);
    }
}
