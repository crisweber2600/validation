using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using Validation.Infrastructure.Repositories;

namespace Validation.Infrastructure.Setup;

public class SetupValidationBuilder
{
    public IServiceCollection Services { get; }

    public SetupValidationBuilder(IServiceCollection services)
    {
        Services = services;
    }

    public SetupValidationBuilder UseSqlServer<TContext>(string connectionString)
        where TContext : DbContext
    {
        Services.AddDbContext<TContext>(o => o.UseSqlServer(connectionString));
        Services.AddScoped<DbContext>(sp => sp.GetRequiredService<TContext>());
        Services.AddScoped<ISaveAuditRepository, EfCoreSaveAuditRepository>();
        return this;
    }

    public SetupValidationBuilder UseMongo(IMongoDatabase database)
    {
        Services.AddSingleton(database);
        Services.AddScoped<ISaveAuditRepository, MongoSaveAuditRepository>();
        return this;
    }

    public IServiceCollection Apply() => Services;
}
