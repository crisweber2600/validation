using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using Microsoft.EntityFrameworkCore;
using Validation.Domain.Entities;
using Validation.Domain.Validation;
using Validation.Infrastructure.DI;
using Validation.Infrastructure.Repositories;

namespace Validation.Tests;

public class SetupValidationExtensionsTests
{
    [Fact]
    public void AddSetupValidation_registers_plan_and_ef_repository()
    {
        var services = new ServiceCollection();
        services.AddDbContext<TestDbContext>(o => o.UseInMemoryDatabase("test"));
        services.AddScoped<DbContext>(sp => sp.GetRequiredService<TestDbContext>());

        services.AddSetupValidation<Item>(builder =>
        {
            builder.SetupDatabase<TestDbContext>("test");
        }, i => i.Metric, ThresholdType.RawDifference, 5m);

        using var provider = services.BuildServiceProvider();
        var repo = provider.GetRequiredService<ISaveAuditRepository>();
        Assert.IsType<EfCoreSaveAuditRepository>(repo);

        var planProvider = provider.GetRequiredService<IValidationPlanProvider>();
        var plan = planProvider.GetPlan(typeof(Item));
        Assert.Equal(ThresholdType.RawDifference, plan.ThresholdType);
        Assert.Equal(5m, plan.ThresholdValue);
        Assert.NotNull(plan.MetricSelector);
    }

    [Fact]
    public void AddSetupValidation_mongo_registers_mongo_repository()
    {
        var services = new ServiceCollection();
        services.AddSetupValidation<Item>(builder =>
        {
            builder.SetupMongoDatabase("mongodb://localhost:27017", "db");
        }, i => i.Metric);

        using var provider = services.BuildServiceProvider();
        var repo = provider.GetRequiredService<ISaveAuditRepository>();
        Assert.IsType<MongoSaveAuditRepository>(repo);
    }
}
