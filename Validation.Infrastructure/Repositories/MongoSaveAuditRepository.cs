using MongoDB.Driver;

namespace Validation.Infrastructure.Repositories;

public class MongoSaveAuditRepository : ISaveAuditRepository
{
    private readonly IMongoCollection<SaveAudit> _collection;

    public MongoSaveAuditRepository(IMongoDatabase database)
    {
        _collection = database.GetCollection<SaveAudit>("saveAudits");
    }

    public async Task AddAsync(SaveAudit entity, CancellationToken ct = default)
    {
        await _collection.InsertOneAsync(entity, null, ct);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var idString = id.ToString();
        await _collection.DeleteOneAsync(x => x.Id == idString, ct);
    }

    public async Task<SaveAudit?> GetAsync(Guid id, CancellationToken ct = default)
    {
        var idString = id.ToString();
        var result = await _collection.Find(x => x.Id == idString).FirstOrDefaultAsync(ct);
        return result;
    }

    public async Task UpdateAsync(SaveAudit entity, CancellationToken ct = default)
    {
        await _collection.ReplaceOneAsync(x => x.Id == entity.Id, entity, cancellationToken: ct);
    }

    public async Task<SaveAudit?> GetLastAsync(string entityId, CancellationToken ct = default)
    {
        return await _collection
            .Find(x => x.EntityId == entityId)
            .SortByDescending(x => x.Timestamp)
            .FirstOrDefaultAsync(ct);
    }
    
    public async Task<SaveAudit?> GetLastAsync(Guid entityId, CancellationToken ct = default)
    {
        return await GetLastAsync(entityId.ToString(), ct);
    }

    public async Task<IEnumerable<SaveAudit>> GetByEntityTypeAsync(string entityType, CancellationToken ct = default)
    {
        return await _collection
            .Find(x => x.EntityType == entityType)
            .SortByDescending(x => x.Timestamp)
            .ToListAsync(ct);
    }

    public async Task<IEnumerable<SaveAudit>> GetByApplicationAsync(string applicationName, CancellationToken ct = default)
    {
        return await _collection
            .Find(x => x.ApplicationName == applicationName)
            .SortByDescending(x => x.Timestamp)
            .ToListAsync(ct);
    }

    public async Task<IEnumerable<SaveAudit>> GetByTimeRangeAsync(DateTime from, DateTime to, CancellationToken ct = default)
    {
        return await _collection
            .Find(x => x.Timestamp >= from && x.Timestamp <= to)
            .SortByDescending(x => x.Timestamp)
            .ToListAsync(ct);
    }

    public async Task<IEnumerable<SaveAudit>> GetByCorrelationIdAsync(string correlationId, CancellationToken ct = default)
    {
        return await _collection
            .Find(x => x.CorrelationId == correlationId)
            .SortByDescending(x => x.Timestamp)
            .ToListAsync(ct);
    }
}
