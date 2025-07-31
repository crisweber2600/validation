using MongoDB.Driver;
using Validation.Domain.Repositories;
using Validation.Domain.Validation;

namespace Validation.Infrastructure.Repositories;

public class MongoGenericRepository<T> : IGenericRepository<T>
{
    private readonly IMongoCollection<T> _collection;
    private readonly ISummarisationValidator _validator;
    private readonly IValidationPlanProvider _planProvider;
    private readonly List<T> _pending = new();

    public MongoGenericRepository(IMongoDatabase database, ISummarisationValidator validator, IValidationPlanProvider planProvider)
    {
        _collection = database.GetCollection<T>(typeof(T).Name.ToLowerInvariant());
        _validator = validator;
        _planProvider = planProvider;
    }

    public Task AddAsync(T item, CancellationToken ct = default)
    {
        _pending.Add(item);
        return Task.CompletedTask;
    }

    public Task AddManyAsync(IEnumerable<T> items, CancellationToken ct = default)
    {
        _pending.AddRange(items);
        return Task.CompletedTask;
    }

    public async Task SaveChangesWithPlanAsync(CancellationToken ct = default)
    {
        var rules = _planProvider.GetRules<T>();
        _validator.Validate(0, 0, rules);

        if (_pending.Count > 0)
        {
            await _collection.InsertManyAsync(_pending, cancellationToken: ct);
            _pending.Clear();
        }
    }
}
