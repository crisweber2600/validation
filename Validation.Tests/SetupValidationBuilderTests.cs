using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using MongoDB.Driver;
using Validation.Infrastructure.DI;
using Validation.Infrastructure.Repositories;
using Validation.Infrastructure.Setup;

namespace Validation.Tests;

public class SetupValidationBuilderTests
{
    private class BuilderDbContext : DbContext
    {
        public BuilderDbContext(DbContextOptions<BuilderDbContext> options) : base(options) { }
    }

    [Fact]
    public void UseSqlServer_registers_context_and_repository()
    {
        var services = new ServiceCollection();
        services.SetupValidation(b => b.UseSqlServer<BuilderDbContext>("Data Source=test;"));

        using var provider = services.BuildServiceProvider();
        Assert.NotNull(provider.GetService<BuilderDbContext>());
        Assert.NotNull(provider.GetService<ISaveAuditRepository>());
    }

    [Fact]
    public void UseMongo_registers_database_and_repository()
    {
        var services = new ServiceCollection();
        var db = new MongoClient().GetDatabase("test-db");
        services.SetupValidation(b => b.UseMongo(db));

        using var provider = services.BuildServiceProvider();
        Assert.Equal(db, provider.GetService<IMongoDatabase>());
        Assert.IsType<MongoSaveAuditRepository>(provider.GetRequiredService<ISaveAuditRepository>());
    }
}
