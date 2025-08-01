using MongoDB.Driver;
using Validation.Domain;

namespace Validation.Infrastructure.Repositories;

public class MongoNannyRecordRepository : INannyRecordRepository
{
    private readonly IMongoCollection<NannyRecord> _collection;

    public MongoNannyRecordRepository(IMongoDatabase database)
    {
        _collection = database.GetCollection<NannyRecord>("nannyRecords");
    }

    public async Task AddAsync(NannyRecord entity, CancellationToken ct = default)
    {
        await _collection.InsertOneAsync(entity, null, ct);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        await _collection.DeleteOneAsync(x => x.Id == id, ct);
    }

    public async Task<NannyRecord?> GetAsync(Guid id, CancellationToken ct = default)
    {
        return await _collection.Find(x => x.Id == id).FirstOrDefaultAsync(ct);
    }

    public async Task UpdateAsync(NannyRecord entity, CancellationToken ct = default)
    {
        await _collection.ReplaceOneAsync(x => x.Id == entity.Id, entity, cancellationToken: ct);
    }

    public async Task<NannyRecord?> GetLastAsync(Guid entityId, CancellationToken ct = default)
    {
        return await _collection
            .Find(x => x.EntityId == entityId)
            .SortByDescending(x => x.Timestamp)
            .FirstOrDefaultAsync(ct);
    }
}
