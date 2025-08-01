using Microsoft.EntityFrameworkCore;
using MassTransit.Testing;
using Validation.Domain.Entities;
using Validation.Infrastructure.Events;
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
    public async Task AddAsync_Publishes_SaveRequested()
    {
        var harness = new InMemoryTestHarness();
        await harness.Start();
        try
        {
            var options = new DbContextOptionsBuilder<TestDbContext>()
                .UseInMemoryDatabase("addasync")
                .Options;
            using var context = new TestDbContext(options);
            var rule = new CountingRule();
            var planProvider = new TestPlanProvider(rule);
            var validator = new SummarisationValidator();
            var repo = new EfGenericRepository<Item>(context, planProvider, validator, harness.Bus);

            await repo.AddAsync(new Item(1));

            Assert.True(await harness.Published.Any<SaveRequested<Item>>());
        }
        finally
        {
            await harness.Stop();
        }
    }

    [Fact]
    public async Task AddManyAsync_Publishes_SaveBatchRequested()
    {
        var harness = new InMemoryTestHarness();
        await harness.Start();
        try
        {
            var options = new DbContextOptionsBuilder<TestDbContext>()
                .UseInMemoryDatabase("addmany")
                .Options;
            using var context = new TestDbContext(options);
            var rule = new CountingRule();
            var planProvider = new TestPlanProvider(rule);
            var validator = new SummarisationValidator();
            var repo = new EfGenericRepository<Item>(context, planProvider, validator, harness.Bus);

            var items = new[] { new Item(1), new Item(2) };
            await repo.AddManyAsync(items);

            Assert.True(await harness.Published.Any<SaveBatchRequested<Item>>());
        }
        finally
        {
            await harness.Stop();
        }
    }
}