using Microsoft.EntityFrameworkCore;
using Validation.Domain.Entities;
using Validation.Domain.Validation;
using Validation.Infrastructure.Repositories;

namespace Validation.Tests;

public class SoftDeleteTests
{
    [Fact]
    public async Task SoftDelete_marks_entity_invalid_in_ef_repository()
    {
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase("softdelete")
            .Options;
        using var context = new TestDbContext(options);
        var planProvider = new InMemoryValidationPlanProvider();
        var validator = new SummarisationValidator();
        var repo = new EfGenericRepository<Item>(context, planProvider, validator);

        var item = new Item(5);
        await repo.AddAsync(item);
        await repo.SaveChangesWithPlanAsync();

        Assert.NotNull(await repo.GetAsync(item.Id));

        await repo.SoftDeleteAsync(item.Id);
        await repo.SaveChangesWithPlanAsync();

        Assert.Null(await repo.GetAsync(item.Id));
        var stored = await context.Items.FindAsync(item.Id);
        Assert.NotNull(stored);
        Assert.False(stored!.Validated);
    }

    [Fact]
    public async Task HardDelete_removes_entity_from_ef_repository()
    {
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase("harddelete")
            .Options;
        using var context = new TestDbContext(options);
        var planProvider = new InMemoryValidationPlanProvider();
        var validator = new SummarisationValidator();
        var repo = new EfGenericRepository<Item>(context, planProvider, validator);

        var item = new Item(3);
        await repo.AddAsync(item);
        await repo.SaveChangesWithPlanAsync();

        await repo.HardDeleteAsync(item.Id);
        await repo.SaveChangesWithPlanAsync();

        Assert.Null(await repo.GetAsync(item.Id));
        var raw = await context.Items.FindAsync(item.Id);
        Assert.Null(raw);
    }
}
