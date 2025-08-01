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

    [Fact]
    public async Task SoftDelete_marks_entity_invalid_and_excludes_from_get()
    {
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase("softdelete")
            .Options;
        using var context = new TestDbContext(options);
        var repo = new EfGenericRepository<Item>(context, new TestPlanProvider(new CountingRule()), new SummarisationValidator());

        var item = new Item(5);
        await repo.AddAsync(item);
        await repo.SaveChangesWithPlanAsync();

        await repo.SoftDeleteAsync(item.Id);
        await repo.SaveChangesWithPlanAsync();

        Assert.False(item.Validated);
        Assert.NotNull(await context.Items.FindAsync(item.Id));
        Assert.Null(await repo.GetAsync(item.Id));
    }

    [Fact]
    public async Task HardDelete_removes_entity()
    {
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase("harddelete")
            .Options;
        using var context = new TestDbContext(options);
        var repo = new EfGenericRepository<Item>(context, new TestPlanProvider(new CountingRule()), new SummarisationValidator());

        var item = new Item(5);
        await repo.AddAsync(item);
        await repo.SaveChangesWithPlanAsync();

        await repo.HardDeleteAsync(item.Id);
        await repo.SaveChangesWithPlanAsync();

        var entity = await context.Items.FindAsync(item.Id);
        Assert.Null(entity);
    }
}