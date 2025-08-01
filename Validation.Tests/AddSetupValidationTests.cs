using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using MongoDB.Driver;
using System.Linq;
using Validation.Domain.Entities;
using Validation.Domain.Validation;
using Validation.Infrastructure.DI;
using Validation.Infrastructure.Repositories;

namespace Validation.Tests;

public class AddSetupValidationTests
{
    [Fact]
    public void AddSetupValidation_registers_plan_and_ef_repository_by_default()
    {
        var services = new ServiceCollection();
        services.AddDbContext<TestDbContext>(o => o.UseInMemoryDatabase("setup1"));
        services.AddScoped<DbContext>(sp => sp.GetRequiredService<TestDbContext>());
        services.AddSetupValidation<Item>(b => { }, i => i.Metric, ThresholdType.RawDifference, 5m);

        var descriptor = services.First(d => d.ServiceType == typeof(ISaveAuditRepository));
        Assert.Equal(typeof(EfCoreSaveAuditRepository), descriptor.ImplementationType);

        var planDescriptor = services.LastOrDefault(d => d.ServiceType == typeof(IValidationPlanProvider) && d.ImplementationInstance != null);
        Assert.NotNull(planDescriptor?.ImplementationInstance);
    }

    [Fact]
    public void AddSetupValidation_registers_mongo_repository_when_configured()
    {
        var services = new ServiceCollection();
        var client = new MongoClient("mongodb://localhost:27017");
        var database = client.GetDatabase("setup2");
        services.AddSingleton(database);
        services.AddSetupValidation<Item>(b => b.UseMongo(), i => i.Metric);

        var descriptor = services.First(d => d.ServiceType == typeof(ISaveAuditRepository));
        Assert.Equal(typeof(MongoSaveAuditRepository), descriptor.ImplementationType);
    }
}
