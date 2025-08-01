using Validation.Domain;
using Validation.Infrastructure.Repositories;

namespace Validation.Tests;

public class InMemoryNannyRecordRepository : INannyRecordRepository
{
    public List<NannyRecord> Records { get; } = new();

    public Task AddAsync(NannyRecord entity, CancellationToken ct = default)
    {
        Records.Add(entity);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        Records.RemoveAll(r => r.Id == id);
        return Task.CompletedTask;
    }

    public Task<NannyRecord?> GetAsync(Guid id, CancellationToken ct = default)
    {
        return Task.FromResult<NannyRecord?>(Records.FirstOrDefault(r => r.Id == id));
    }

    public Task UpdateAsync(NannyRecord entity, CancellationToken ct = default)
    {
        var index = Records.FindIndex(r => r.Id == entity.Id);
        if (index >= 0) Records[index] = entity;
        return Task.CompletedTask;
    }

    public Task<NannyRecord?> GetLastAsync(Guid entityId, CancellationToken ct = default)
    {
        var rec = Records.Where(r => r.EntityId == entityId)
            .OrderByDescending(r => r.Timestamp)
            .FirstOrDefault();
        return Task.FromResult<NannyRecord?>(rec);
    }
}
