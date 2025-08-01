using MongoDB.Driver;
using Validation.Domain.Entities;
using Validation.Domain.Validation;

namespace Validation.Infrastructure.Repositories;

public class MongoGenericRepository<T> : IGenericRepository<T> where T : BaseEntity
{
    private readonly IMongoCollection<T> _collection;
    private readonly IValidationPlanProvider _planProvider;
    private readonly SummarisationValidator _validator;

    public MongoGenericRepository(IMongoDatabase database, IValidationPlanProvider planProvider, SummarisationValidator validator)
    {
        _collection = database.GetCollection<T>(typeof(T).Name.ToLowerInvariant());
        _planProvider = planProvider;
        _validator = validator;
    }

    public async Task AddAsync(T entity, CancellationToken ct = default)
    {
        await _collection.InsertOneAsync(entity, cancellationToken: ct);
    }

    public async Task AddManyAsync(IEnumerable<T> items, CancellationToken ct = default)
    {
        await _collection.InsertManyAsync(items, cancellationToken: ct);
    }

    public async Task<T?> GetAsync(Guid id, CancellationToken ct = default)
    {
        var filter = Builders<T>.Filter.Eq(e => e.Id, id) & Builders<T>.Filter.Eq(e => e.Validated, true);
        return await _collection.Find(filter).FirstOrDefaultAsync(ct);
    }

    public async Task UpdateAsync(T entity, CancellationToken ct = default)
    {
        var idProperty = typeof(T).GetProperty("Id");
        if (idProperty == null) throw new InvalidOperationException("Entity must have Id property");
        var id = (Guid)idProperty.GetValue(entity)!;
        var filter = Builders<T>.Filter.Eq("Id", id);
        await _collection.ReplaceOneAsync(filter, entity, cancellationToken: ct);
    }

    public async Task SoftDeleteAsync(Guid id, CancellationToken ct = default)
    {
        var update = Builders<T>.Update.Set(e => e.Validated, false);
        await _collection.UpdateOneAsync(e => e.Id == id, update, cancellationToken: ct);
    }

    public async Task HardDeleteAsync(Guid id, CancellationToken ct = default)
    {
        await _collection.DeleteOneAsync(e => e.Id == id, ct);
    }

    public async Task SaveChangesWithPlanAsync(CancellationToken ct = default)
    {
        var rules = _planProvider.GetRules<T>();
        _validator.Validate(0, 0, rules);
        await Task.CompletedTask;
    }
}