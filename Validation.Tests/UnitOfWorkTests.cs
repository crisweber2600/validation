using Microsoft.EntityFrameworkCore;
using Validation.Domain.Entities;
using Validation.Domain.Validation;
using System.Linq;
using Validation.Infrastructure.UnitOfWork;
using Validation.Infrastructure;

namespace Validation.Tests;

public class UnitOfWorkTests
{
    private class TestPlanProvider : IValidationPlanProvider
    {
        public IEnumerable<IValidationRule> GetRules<T>() => new[] { new RawDifferenceRule(100) };
    }

    [Fact]
    public async Task SaveChangesWithPlan_sets_validation_result()
    {
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase("uow-test")
            .Options;
        using var context = new TestDbContext(options);

        var entityId = Guid.NewGuid();
        context.SaveAudits.Add(new SaveAudit { Id = Guid.NewGuid(), EntityId = entityId, Metric = 50m });
        await context.SaveChangesAsync();

        var uow = new UnitOfWork<TestDbContext>(context, new TestPlanProvider(), new SummarisationValidator());
        var repo = uow.Repository<SaveAudit>();
        await repo.AddAsync(new SaveAudit { Id = Guid.NewGuid(), EntityId = entityId, Metric = 60m });

        await uow.SaveChangesWithPlanAsync<Item>();

        var latest = context.SaveAudits.OrderByDescending(a => a.Timestamp).First();
        Assert.True(latest.IsValid);
    }
}
