using Microsoft.EntityFrameworkCore;
using MassTransit.Testing;
using Validation.Domain.Entities;
using Validation.Domain.Events;
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
    public async Task AddMany_SaveChanges_Publishes_batch_event_and_validates_once()
    {
        var harness = new InMemoryTestHarness();
        await harness.Start();
        try
        {
            var options = new DbContextOptionsBuilder<TestDbContext>()
                .UseInMemoryDatabase("genericrepo")
                .Options;
            using var context = new TestDbContext(options);
            var rule = new CountingRule();
            var planProvider = new TestPlanProvider(rule);
            var validator = new SummarisationValidator();
            var repo = new EfGenericRepository<Item>(context, planProvider, validator, harness.Bus);

            var items = Enumerable.Range(0, 100).Select(i => new Item(i));
            await repo.AddManyAsync(items);
            await repo.SaveChangesWithPlanAsync();

            Assert.Equal(1, rule.Calls);
            Assert.True(await harness.Published.Any<SaveBatchRequested<Item>>());
        }
        finally
        {
            await harness.Stop();
        }
    }

    [Fact]
    public async Task AddAsync_publishes_event()
    {
        var harness = new InMemoryTestHarness();
        await harness.Start();
        try
        {
            var options = new DbContextOptionsBuilder<TestDbContext>()
                .UseInMemoryDatabase("genericrepo_add")
                .Options;
            using var context = new TestDbContext(options);
            var repo = new EfGenericRepository<Item>(context, new TestPlanProvider(new CountingRule()), new SummarisationValidator(), harness.Bus);
            await repo.AddAsync(new Item(5));

            Assert.True(await harness.Published.Any<SaveRequested<Item>>());
        }
        finally
        {
            await harness.Stop();
        }
    }
}