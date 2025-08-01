using MongoDB.Driver;
using Validation.Domain.Entities;
using Validation.Domain.Repositories;

namespace Validation.Infrastructure.Repositories;

public class MongoSummaryRecordRepository : ISummaryRecordRepository
{
    private readonly IMongoCollection<SummaryRecord> _collection;

    public MongoSummaryRecordRepository(IMongoDatabase database)
    {
        _collection = database.GetCollection<SummaryRecord>("summaryrecords");
    }

    public async Task AddAsync(SummaryRecord record, CancellationToken ct = default)
    {
        await _collection.InsertOneAsync(record, null, ct);
    }

    public async Task<decimal?> GetLatestValueAsync(string programName, string entity, CancellationToken ct = default)
    {
        var filter = Builders<SummaryRecord>.Filter.Eq(x => x.ProgramName, programName) &
                     Builders<SummaryRecord>.Filter.Eq(x => x.Entity, entity);
        var result = await _collection
            .Find(filter)
            .SortByDescending(x => x.RecordedAt)
            .FirstOrDefaultAsync(ct);
        return result?.MetricValue;
    }
}
