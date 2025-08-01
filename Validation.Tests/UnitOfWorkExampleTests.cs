using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Validation.Domain.Validation;
using Validation.Infrastructure;

namespace Validation.Tests;

public class UnitOfWorkExampleTests
{
    private class YourEntity : Validation.Domain.Entities.BaseEntity
    {
    }

    private class ExampleDbContext : DbContext
    {
        public ExampleDbContext(DbContextOptions<ExampleDbContext> options) : base(options) { }
        public DbSet<SaveAudit> SaveAudits => Set<SaveAudit>();
        public DbSet<YourEntity> Entities => Set<YourEntity>();
    }

    [Fact]
    public async Task UnitOfWork_usage_example()
    {
        var services = new ServiceCollection();
        services.AddDbContext<ExampleDbContext>(o => o.UseInMemoryDatabase("uowexample"));
        services.AddScoped<DbContext>(sp => sp.GetRequiredService<ExampleDbContext>());
        services.AddSingleton<IValidationPlanProvider, InMemoryValidationPlanProvider>();
        services.AddScoped<SummarisationValidator>();
        services.AddScoped<UnitOfWork>();

        var provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();
        var planProvider = scope.ServiceProvider.GetRequiredService<IValidationPlanProvider>();
        planProvider.AddPlan<YourEntity>(new ValidationPlan(_ => 0m, ThresholdType.RawDifference, 5));

        var uow = scope.ServiceProvider.GetRequiredService<UnitOfWork>();
        await uow.Repository<YourEntity>().AddAsync(new YourEntity());
        var count = await uow.SaveChangesWithPlanAsync<YourEntity>();

        var ctx = scope.ServiceProvider.GetRequiredService<ExampleDbContext>();
        Assert.Equal(1, ctx.Entities.Count());
        Assert.Equal(1, count);
    }
}