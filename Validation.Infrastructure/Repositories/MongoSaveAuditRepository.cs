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
        await _collection.DeleteOneAsync(x => x.Id == id, ct);
    }

    public async Task<SaveAudit?> GetAsync(Guid id, CancellationToken ct = default)
    {
        var result = await _collection.Find(x => x.Id == id).FirstOrDefaultAsync(ct);
        return result;
    }

    public async Task UpdateAsync(SaveAudit entity, CancellationToken ct = default)
    {
        await _collection.ReplaceOneAsync(x => x.Id == entity.Id, entity, cancellationToken: ct);
    }
}
