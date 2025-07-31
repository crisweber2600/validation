using MongoDB.Driver;
using Validation.Domain.Validation;

namespace Validation.Infrastructure.Repositories;

public class MongoGenericRepository<T> : IGenericRepository<T>
{
    private readonly IMongoCollection<T> _collection;
    private readonly ISummarisationValidator _validator;
    private readonly IValidationPlanProvider _planProvider;
    private readonly List<T> _buffer = new();

    public MongoGenericRepository(IMongoDatabase database, ISummarisationValidator validator, IValidationPlanProvider planProvider)
    {
        _collection = database.GetCollection<T>(typeof(T).Name.ToLowerInvariant());
        _validator = validator;
        _planProvider = planProvider;
    }

    public Task AddAsync(T entity, CancellationToken ct = default)
    {
        _buffer.Add(entity);
        return Task.CompletedTask;
    }

    public Task AddManyAsync(IEnumerable<T> items, CancellationToken ct = default)
    {
        _buffer.AddRange(items);
        return Task.CompletedTask;
    }

    public async Task<T?> GetAsync(Guid id, CancellationToken ct = default)
    {
        return await _collection.Find(Builders<T>.Filter.Eq("Id", id)).FirstOrDefaultAsync(ct);
    }

    public Task UpdateAsync(T entity, CancellationToken ct = default)
    {
        return _collection.ReplaceOneAsync(Builders<T>.Filter.Eq("Id", GetId(entity)), entity, cancellationToken: ct);
    }

    public Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        return _collection.DeleteOneAsync(Builders<T>.Filter.Eq("Id", id), ct);
    }

    private static Guid GetId(T entity)
    {
        var prop = typeof(T).GetProperty("Id");
        return prop != null ? (Guid)(prop.GetValue(entity) ?? Guid.Empty) : Guid.Empty;
    }

    public async Task SaveChangesWithPlanAsync(CancellationToken ct = default)
    {
        if (_buffer.Count > 0)
        {
            await _collection.InsertManyAsync(_buffer, cancellationToken: ct);
            _buffer.Clear();
        }
        _validator.Validate(0, 0, _planProvider.GetRules<T>());
    }
}
