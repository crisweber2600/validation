using Microsoft.EntityFrameworkCore;
using Validation.Domain.Entities;
using Validation.Domain.Validation;
using Validation.Infrastructure.Repositories;
using Validation.Infrastructure.UnitOfWork;
using Xunit;

namespace Validation.Tests;

public class UnitOfWorkTests
{
    private class TestPlanProvider : IValidationPlanProvider
    {
        public IEnumerable<IValidationRule> GetRules<T>() => new[] { new RawDifferenceRule(50) };
    }

    private static UnitOfWork<TestDbContext> CreateUow(string db)
    {
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(db)
            .Options;
        var context = new TestDbContext(options);
        return new UnitOfWork<TestDbContext>(context, new SummarisationValidator(), new TestPlanProvider());
    }

    [Fact]
    public async Task SaveChangesWithPlanAsync_valid_succeeds()
    {
        var uow = CreateUow("valid_db");
        var repo = uow.Repository<Item>();
        await repo.AddAsync(new Item(10));
        var result = await uow.SaveChangesWithPlanAsync<Item>();
        Assert.Equal(1, result);
    }

    [Fact]
    public async Task SaveChangesWithPlanAsync_invalid_throws()
    {
        var uow = CreateUow("invalid_db");
        var repo = uow.Repository<Item>();
        var item = new Item(10);
        await repo.AddAsync(item);
        await uow.SaveChangesWithPlanAsync<Item>();
        item.UpdateMetric(100);
        await Assert.ThrowsAsync<InvalidOperationException>(() => uow.SaveChangesWithPlanAsync<Item>());
    }
}
