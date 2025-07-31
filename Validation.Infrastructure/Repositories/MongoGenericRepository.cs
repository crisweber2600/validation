using MongoDB.Driver;
using Validation.Domain.Validation;

namespace Validation.Infrastructure.Repositories;

public class MongoGenericRepository<T> : IGenericRepository<T>
{
    private readonly IMongoCollection<T> _collection;
    private readonly List<T> _pending = new();

    public MongoGenericRepository(IMongoDatabase database)
    {
        _collection = database.GetCollection<T>(typeof(T).Name.ToLowerInvariant());
    }

    public Task AddAsync(T entity, CancellationToken ct = default)
    {
        _pending.Add(entity);
        return Task.CompletedTask;
    }

    public Task AddManyAsync(IEnumerable<T> items, CancellationToken ct = default)
    {
        _pending.AddRange(items);
        return Task.CompletedTask;
    }

    public async Task SaveChangesWithPlanAsync(IValidationPlanProvider planProvider, SummarisationValidator validator, CancellationToken ct = default)
    {
        var rules = planProvider.GetRules<T>();
        if (_pending.Count > 0)
        {
            validator.Validate(0, 0, rules);
            await _collection.InsertManyAsync(_pending, cancellationToken: ct);
            _pending.Clear();
        }
    }
}
