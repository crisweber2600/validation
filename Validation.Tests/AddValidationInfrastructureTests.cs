using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using MongoDB.Driver;
using Validation.Infrastructure.DI;
using Validation.Infrastructure.Repositories;

namespace Validation.Tests;

public class AddValidationInfrastructureTests
{
    [Fact]
    public void AddValidationInfrastructure_registers_nanny_repository()
    {
        var services = new ServiceCollection();
        services.AddDbContext<TestDbContext>(o => o.UseInMemoryDatabase("nanny-di"));
        services.AddScoped<DbContext>(sp => sp.GetRequiredService<TestDbContext>());
        services.AddValidationInfrastructure();

        using var provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();
        var repo = scope.ServiceProvider.GetService<INannyRecordRepository>();
        Assert.NotNull(repo);
    }

    [Fact]
    public void AddMongoValidationInfrastructure_registers_nanny_repository()
    {
        var client = new MongoClient("mongodb://localhost:27017");
        var db = client.GetDatabase("testdb");
        var services = new ServiceCollection();
        services.AddMongoValidationInfrastructure(db);

        using var provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();
        var repo = scope.ServiceProvider.GetService<INannyRecordRepository>();
        Assert.NotNull(repo);
    }
}
