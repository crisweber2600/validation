using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using Validation.Domain.Entities;
using Validation.Domain.Validation;
using Validation.Infrastructure.DI;
using Validation.Infrastructure.Repositories;

namespace Validation.Tests;

public class SetupValidationBuilderTests
{
    [Fact]
    public void UseSqlServer_registers_dbcontext_and_repository()
    {
        var services = new ServiceCollection();
        services.SetupValidation(b =>
        {
            b.UseSqlServer<TestDbContext>("Server=(localdb)\\mssqllocaldb;Database=TestDb;Trusted_Connection=True;");
        });

        using var provider = services.BuildServiceProvider();
        Assert.NotNull(provider.GetService<TestDbContext>());
        Assert.IsType<EfCoreSaveAuditRepository>(provider.GetRequiredService<ISaveAuditRepository>());
    }

    [Fact]
    public void UseMongo_registers_database_and_repository()
    {
        var services = new ServiceCollection();
        var client = new MongoClient("mongodb://localhost:27017");
        var database = client.GetDatabase("testdb");
        services.SetupValidation(b =>
        {
            b.UseMongo(database);
        });

        using var provider = services.BuildServiceProvider();
        Assert.Same(database, provider.GetRequiredService<IMongoDatabase>());
        Assert.IsType<MongoSaveAuditRepository>(provider.GetRequiredService<ISaveAuditRepository>());
    }

    [Fact]
    public void AddPlan_registers_plan_with_provider()
    {
        var services = new ServiceCollection();
        var client = new MongoClient("mongodb://localhost:27017");
        var database = client.GetDatabase("testdb");
        var plan = new ValidationPlan(e => ((Item)e).Metric, ThresholdType.RawDifference, 1m);

        services.SetupValidation(b =>
        {
            b.UseMongo(database);
            b.AddPlan<Item>(plan);
        });

        using var provider = services.BuildServiceProvider();
        var planProvider = provider.GetRequiredService<IValidationPlanProvider>();
        var result = planProvider.GetPlan(typeof(Item));
        Assert.Equal(plan.ThresholdValue, result.ThresholdValue);
    }
}
