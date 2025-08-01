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
        var rec = await _collection
            .Find(r => r.ProgramName == programName && r.Entity == entity)
            .SortByDescending(r => r.RecordedAt)
            .FirstOrDefaultAsync(ct);
        return rec?.MetricValue;
    }
}
