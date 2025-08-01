using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MassTransit.Testing;
using Validation.Domain.Validation;
using Validation.Infrastructure;
using ValidationFlow.Messages.Batch;

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
        services.AddDbContext<ExampleDbContext>(o => o.UseInMemoryDatabase("uowexample"));
        services.AddScoped<DbContext>(sp => sp.GetRequiredService<ExampleDbContext>());
        services.AddSingleton<IValidationPlanProvider, InMemoryValidationPlanProvider>();
        services.AddScoped<SummarisationValidator>();
        var provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();
        var planProvider = scope.ServiceProvider.GetRequiredService<IValidationPlanProvider>();
        planProvider.AddPlan<YourEntity>(new ValidationPlan(e => ((YourEntity)e).Id, ThresholdType.RawDifference, 5));

        var harness = new InMemoryTestHarness();
        await harness.Start();
        try
        {
            var db = scope.ServiceProvider.GetRequiredService<ExampleDbContext>();
            var validator = scope.ServiceProvider.GetRequiredService<SummarisationValidator>();
            var uow = new UnitOfWork(db, planProvider, validator, harness.Bus);
            await uow.Repository<YourEntity>().AddAsync(new YourEntity { Id = 50 });

            Assert.True(await harness.Published.Any<ValidationFlow.Messages.Batch.SaveRequested<YourEntity>>());
        }
        finally
        {
            await harness.Stop();
        }
    }
}