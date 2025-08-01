using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MassTransit.Testing;
using MassTransit;
using Validation.Domain.Events;
using Validation.Domain.Validation;
using Validation.Infrastructure;

namespace Validation.Tests;

public class UnitOfWorkExampleTests
{
    private class YourEntity
    {
        public int Id { get; set; }
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
        services.AddMassTransitTestHarness();
        services.AddDbContext<ExampleDbContext>(o => o.UseInMemoryDatabase("uowexample"));
        services.AddScoped<DbContext>(sp => sp.GetRequiredService<ExampleDbContext>());
        services.AddSingleton<IValidationPlanProvider, InMemoryValidationPlanProvider>();
        services.AddScoped<SummarisationValidator>();
        services.AddScoped<UnitOfWork>();

        var provider = services.BuildServiceProvider(true);
        var harness = provider.GetRequiredService<ITestHarness>();
        await harness.Start();
        try
        {
            using var scope = provider.CreateScope();
        var planProvider = scope.ServiceProvider.GetRequiredService<IValidationPlanProvider>();
        planProvider.AddPlan<YourEntity>(new ValidationPlan(e => ((YourEntity)e).Id, ThresholdType.RawDifference, 5));

        var uow = scope.ServiceProvider.GetRequiredService<UnitOfWork>();
        await uow.Repository<YourEntity>().AddAsync(new YourEntity { Id = 50 });
        await uow.SaveChangesWithPlanAsync<YourEntity>();

        Assert.True(await harness.Published.Any<SaveRequested<YourEntity>>());
        }
        finally
        {
            await harness.Stop();
        }
    }
}