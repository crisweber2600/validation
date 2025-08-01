using Microsoft.Extensions.DependencyInjection;
using Validation.Domain.Entities;
using Validation.Domain.Validation;
using Validation.Infrastructure.DI;
using Validation.Infrastructure.Repositories;
using MongoDB.Driver;

namespace Validation.Tests;

public class SetupValidationExtensionsTests
{
    [Fact]
    public void AddSetupValidation_registers_ef_repository_and_plan()
    {
        var services = new ServiceCollection();
        services.AddSetupValidation<Item>(b => b.UseEntityFramework<TestDbContext>("test"), i => i.Metric);

        using var provider = services.BuildServiceProvider();
        var repo = provider.GetRequiredService<ISaveAuditRepository>();
        Assert.IsType<EfCoreSaveAuditRepository>(repo);

        var planProvider = provider.GetRequiredService<IValidationPlanProvider>();
        var plan = planProvider.GetPlan(typeof(Item));
        Assert.Equal(ThresholdType.PercentChange, plan.ThresholdType);
        Assert.Equal(0.1m, plan.ThresholdValue);
    }

    [Fact]
    public void AddSetupValidation_registers_mongo_repository()
    {
        var services = new ServiceCollection();
        services.AddSetupValidation<Item>(b => b.UseMongo("mongodb://localhost:27017", "db"), i => i.Metric);

        using var provider = services.BuildServiceProvider();
        var repo = provider.GetRequiredService<ISaveAuditRepository>();
        Assert.IsType<MongoSaveAuditRepository>(repo);
    }
}
