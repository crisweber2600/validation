using Microsoft.EntityFrameworkCore;
using Validation.Domain.Entities;
using Validation.Domain.Validation;
using Validation.Infrastructure;
using Validation.Infrastructure.Repositories;
using Validation.Infrastructure.UnitOfWork;

namespace Validation.Tests;

public class UnitOfWorkTests
{
    private class TestPlanProvider : IValidationPlanProvider
    {
        private readonly decimal _threshold;
        public TestPlanProvider(decimal threshold)
        {
            _threshold = threshold;
        }
        public IEnumerable<IValidationRule> GetRules<T>() => new[] { new RawDifferenceRule(_threshold) };
    }

    [Fact]
    public async Task SaveChangesWithPlanAsync_sets_IsValid_based_on_rules()
    {
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase("uow_valid")
            .Options;
        var context = new TestDbContext(options);
        var previous = new SaveAudit { Id = Guid.NewGuid(), EntityId = Guid.NewGuid(), Metric = 10m };
        context.SaveAudits.Add(previous);
        await context.SaveChangesAsync();

        var uow = new UnitOfWork<TestDbContext>(context, new SummarisationValidator(), new TestPlanProvider(100m));
        var repo = uow.Repository<SaveAudit>();
        var audit = new SaveAudit { Id = Guid.NewGuid(), EntityId = previous.EntityId, Metric = 20m };
        await repo.AddAsync(audit);

        await uow.SaveChangesWithPlanAsync<Item>();

        Assert.True(audit.IsValid);
    }

    [Fact]
    public async Task SaveChangesWithPlanAsync_marks_invalid_when_rule_fails()
    {
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase("uow_invalid")
            .Options;
        var context = new TestDbContext(options);
        var previous = new SaveAudit { Id = Guid.NewGuid(), EntityId = Guid.NewGuid(), Metric = 10m };
        context.SaveAudits.Add(previous);
        await context.SaveChangesAsync();

        var uow = new UnitOfWork<TestDbContext>(context, new SummarisationValidator(), new TestPlanProvider(5m));
        var repo = uow.Repository<SaveAudit>();
        var audit = new SaveAudit { Id = Guid.NewGuid(), EntityId = previous.EntityId, Metric = 20m };
        await repo.AddAsync(audit);

        await uow.SaveChangesWithPlanAsync<Item>();

        Assert.False(audit.IsValid);
    }
}
