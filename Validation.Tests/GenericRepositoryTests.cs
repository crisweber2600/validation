using Microsoft.EntityFrameworkCore;
using Validation.Domain.Entities;
using Validation.Domain.Validation;
using Validation.Infrastructure.Repositories;

namespace Validation.Tests;

public class GenericRepositoryTests
{
    private class CountingRule : IValidationRule
    {
        public int Calls { get; private set; }
        public bool Validate(decimal previousValue, decimal newValue)
        {
            Calls++;
            return true;
        }
    }

    private class TestPlanProvider : IValidationPlanProvider
    {
        private readonly IValidationRule _rule;
        public TestPlanProvider(IValidationRule rule)
        {
            _rule = rule;
        }
        public IEnumerable<IValidationRule> GetRules<T>() => new[] { _rule };
        public ValidationPlan GetPlan(Type t) => new ValidationPlan(new[] { _rule });
        public ValidationPlan GetPlanFor<T>() => GetPlan(typeof(T));
        public void AddPlan<T>(ValidationPlan plan) { }
    }

    [Fact]
    public async Task AddMany_SaveChanges_ValidatesOnce()
    {
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase("genericrepo")
            .Options;
        using var context = new TestDbContext(options);
        var rule = new CountingRule();
        var planProvider = new TestPlanProvider(rule);
        var validator = new SummarisationValidator();
        var repo = new EfGenericRepository<Item>(context, planProvider, validator);

        var items = Enumerable.Range(0, 100).Select(i => new Item(i));
        await repo.AddManyAsync(items);
        await repo.SaveChangesWithPlanAsync();

        Assert.Equal(1, rule.Calls);
        Assert.Equal(100, context.Items.Count());
    }
}