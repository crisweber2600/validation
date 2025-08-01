using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using Validation.Domain.Entities;
using Validation.Domain.Validation;
using Validation.Infrastructure.DI;
using Validation.Infrastructure.Repositories;

namespace Validation.Tests;

public class SetupValidationExtensionsTests
{
    [Fact]
    public void SetupValidation_registers_ef_repository()
    {
        var services = new ServiceCollection();
        services.SetupValidation(b => b.SetupDatabase<TestDbContext>("test"));

        using var provider = services.BuildServiceProvider();
        var repo = provider.GetRequiredService<ISaveAuditRepository>();
        Assert.IsType<EfCoreSaveAuditRepository>(repo);
    }

    [Fact]
    public void SetupValidation_registers_mongo_repository()
    {
        var services = new ServiceCollection();
        var client = new MongoClient("mongodb://localhost");
        var db = client.GetDatabase("testdb");
        services.SetupValidation(b => b.SetupMongoDatabase(db));

        using var provider = services.BuildServiceProvider();
        var repo = provider.GetRequiredService<ISaveAuditRepository>();
        Assert.IsType<MongoSaveAuditRepository>(repo);
    }

    [Fact]
    public void AddSetupValidation_registers_plan_and_repo()
    {
        var services = new ServiceCollection();
        services.AddSetupValidation<Item>(b => b.SetupDatabase<TestDbContext>("db"), i => i.Metric);

        using var provider = services.BuildServiceProvider();
        var repo = provider.GetRequiredService<ISaveAuditRepository>();
        Assert.IsType<EfCoreSaveAuditRepository>(repo);
        var plans = provider.GetRequiredService<IValidationPlanProvider>();
        var plan = plans.GetPlan(typeof(Item));
        Assert.Equal(ThresholdType.PercentChange, plan.ThresholdType);
    }
}
