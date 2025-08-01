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
    public async Task AddAsync_Publishes_SaveRequested()
    {
        var harness = new MassTransit.Testing.InMemoryTestHarness();
        await harness.Start();
        try
        {
            var options = new DbContextOptionsBuilder<TestDbContext>()
                .UseInMemoryDatabase("genericrepo-add")
                .Options;
            using var context = new TestDbContext(options);
            var repo = new EfGenericRepository<Item>(context, new TestPlanProvider(new CountingRule()), new SummarisationValidator(), harness.Bus);

            await repo.AddAsync(new Item(1));

            Assert.True(await harness.Published.Any<ValidationFlow.Messages.Batch.SaveRequested<Item>>());
        }
        finally
        {
            await harness.Stop();
        }
    }

    [Fact]
    public async Task AddManyAsync_Publishes_SaveBatchRequested()
    {
        var harness = new MassTransit.Testing.InMemoryTestHarness();
        await harness.Start();
        try
        {
            var options = new DbContextOptionsBuilder<TestDbContext>()
                .UseInMemoryDatabase("genericrepo-batch")
                .Options;
            using var context = new TestDbContext(options);
            var repo = new EfGenericRepository<Item>(context, new TestPlanProvider(new CountingRule()), new SummarisationValidator(), harness.Bus);

            var items = Enumerable.Range(0, 5).Select(i => new Item(i)).ToList();
            await repo.AddManyAsync(items);

            Assert.True(await harness.Published.Any<ValidationFlow.Messages.Batch.SaveBatchRequested<Item>>());
        }
        finally
        {
            await harness.Stop();
        }
    }
}