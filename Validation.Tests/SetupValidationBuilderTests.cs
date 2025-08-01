using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using Microsoft.EntityFrameworkCore;
using Validation.Infrastructure.DI;
using Validation.Infrastructure.Repositories;
using Validation.Infrastructure.Setup;
using Xunit;

namespace Validation.Tests;

public class SetupValidationBuilderTests
{
    [Fact]
    public void SetupValidation_UseSqlServer_registers_db_and_repository()
    {
        var services = new ServiceCollection();
        services.SetupValidation(builder =>
        {
            builder.UseSqlServer<TestDbContext>("Server=(local);Database=Test;Trusted_Connection=True;");
        });

        using var provider = services.BuildServiceProvider();
        Assert.IsType<EfCoreSaveAuditRepository>(provider.GetRequiredService<ISaveAuditRepository>());
        Assert.IsType<TestDbContext>(provider.GetRequiredService<DbContext>());
    }

    [Fact]
    public void SetupValidation_UseMongo_registers_database_and_repository()
    {
        var mongo = new MongoClient().GetDatabase("db");
        var services = new ServiceCollection();
        services.SetupValidation(builder =>
        {
            builder.UseMongo(mongo);
        });

        using var provider = services.BuildServiceProvider();
        Assert.IsType<MongoSaveAuditRepository>(provider.GetRequiredService<ISaveAuditRepository>());
        Assert.Same(mongo, provider.GetRequiredService<IMongoDatabase>());
    }
}
