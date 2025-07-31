using Microsoft.EntityFrameworkCore;
using Validation.Domain.Entities;
using Validation.Domain.Validation;
using Validation.Infrastructure.Repositories;

namespace Validation.Tests;

public class TrackingValidator : ISummarisationValidator
{
    public int Calls { get; private set; }
    private readonly SummarisationValidator _inner = new();

    public bool Validate(decimal previousValue, decimal newValue, IEnumerable<IValidationRule> rules)
    {
        Calls++;
        return _inner.Validate(previousValue, newValue, rules);
    }
}

public class GenericRepositoryTests
{
    private class TestPlanProvider : IValidationPlanProvider
    {
        public IEnumerable<IValidationRule> GetRules<T>() => new[] { new AlwaysValidRule() };
    }

    [Fact]
    public async Task AddMany_deferred_until_SaveChanges_calls_validator_once()
    {
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase("generic")
            .Options;
        var context = new TestDbContext(options);
        var validator = new TrackingValidator();
        var provider = new TestPlanProvider();
        var repo = new EfGenericRepository<Item>(context, validator, provider);

        var items = Enumerable.Range(0, 100).Select(i => new Item(i));
        await repo.AddManyAsync(items);

        await repo.SaveChangesWithPlanAsync();

        Assert.Equal(1, validator.Calls);
        Assert.Equal(100, context.Set<Item>().Count());
    }
}
