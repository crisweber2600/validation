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

    public async Task<SaveAudit?> GetLastAuditAsync(string entityId, string propertyName, CancellationToken ct = default)
    {
        return await _collection
            .Find(x => x.EntityId == entityId && x.PropertyName == propertyName)
            .SortByDescending(x => x.Timestamp)
            .FirstOrDefaultAsync(ct);
    }

    public async Task AddOrUpdateAuditAsync(string entityId, string entityType, string propertyName,
                                          decimal propertyValue, bool isValid,
                                          string? applicationName = null, string? operationType = null,
                                          string? correlationId = null, CancellationToken ct = default)
    {
        var filter = Builders<SaveAudit>.Filter.And(
            Builders<SaveAudit>.Filter.Eq(x => x.EntityId, entityId),
            Builders<SaveAudit>.Filter.Eq(x => x.PropertyName, propertyName)
        );

        var existingAudit = await _collection.Find(filter).FirstOrDefaultAsync(ct);
        
        SaveAudit auditToSave;
        if (existingAudit != null)
        {
            // Update existing audit
            existingAudit.PropertyValue = propertyValue;
            existingAudit.IsValid = isValid;
            existingAudit.Timestamp = DateTime.UtcNow;
            existingAudit.ApplicationName = applicationName;
            existingAudit.OperationType = operationType;
            existingAudit.CorrelationId = correlationId;
            auditToSave = existingAudit;
        }
        else
        {
            // Create new audit
            auditToSave = new SaveAudit
            {
                Id = Guid.NewGuid().ToString(),
                EntityId = entityId,
                EntityType = entityType,
                PropertyName = propertyName,
                PropertyValue = propertyValue,
                IsValid = isValid,
                ApplicationName = applicationName,
                OperationType = operationType,
                CorrelationId = correlationId,
                Timestamp = DateTime.UtcNow
            };
        }

        await _collection.ReplaceOneAsync(
            filter,
            auditToSave,
            new ReplaceOptions { IsUpsert = true },
            ct);
    }
}
