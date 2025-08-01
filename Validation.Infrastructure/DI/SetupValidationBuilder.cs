using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using Validation.Infrastructure.Repositories;

namespace Validation.Infrastructure.DI;

public class SetupValidationBuilder
{
    public IServiceCollection Services { get; }

    public SetupValidationBuilder(IServiceCollection services)
    {
        Services = services;
    }

    public IServiceCollection SetupDatabase<TContext>(string connectionString)
        where TContext : DbContext
    {
        Services.AddDbContext<TContext>(o => o.UseInMemoryDatabase(connectionString));
        Services.AddScoped<DbContext>(sp => sp.GetRequiredService<TContext>());
        Services.AddScoped<ISaveAuditRepository, EfCoreSaveAuditRepository>();
        return Services;
    }

    public IServiceCollection SetupMongoDatabase(string connectionString, string dbName)
    {
        var client = new MongoClient(connectionString);
        var database = client.GetDatabase(dbName);
        Services.AddSingleton(database);
        Services.AddScoped<ISaveAuditRepository, MongoSaveAuditRepository>();
        return Services;
    }
}
