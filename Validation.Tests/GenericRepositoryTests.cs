using Microsoft.EntityFrameworkCore;
using Validation.Domain.Entities;
using Validation.Domain.Validation;
using Validation.Infrastructure.Repositories;

namespace Validation.Tests;

public class GenericRepositoryTests
{
    private class CountingRule : IValidationRule
    {
        public int Count { get; private set; }
        public bool Validate(decimal previousValue, decimal newValue)
        {
            Count++;
            return true;
        }
    }

    private class TestPlanProvider : IValidationPlanProvider
    {
        private readonly IValidationRule _rule;
        public TestPlanProvider(IValidationRule rule) => _rule = rule;
        public IEnumerable<IValidationRule> GetRules<T>() => new[] { _rule };
    }

    [Fact]
    public async Task Bulk_insert_validates_once()
    {
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase("bulk")
            .Options;
        using var ctx = new TestDbContext(options);
        var repo = new EfGenericRepository<Item>(ctx);

        var items = Enumerable.Range(0, 100).Select(i => new Item(i));
        await repo.AddManyAsync(items);

        var rule = new CountingRule();
        var provider = new TestPlanProvider(rule);
        var validator = new SummarisationValidator();

        await repo.SaveChangesWithPlanAsync(provider, validator);

        Assert.Equal(1, rule.Count);
        Assert.Equal(100, ctx.Set<Item>().Count());
    }
}
