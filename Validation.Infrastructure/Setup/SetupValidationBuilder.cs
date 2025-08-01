using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using Validation.Infrastructure.Repositories;

namespace Validation.Infrastructure.Setup;

public class SetupValidationBuilder
{
    private readonly IServiceCollection _services;

    public SetupValidationBuilder(IServiceCollection services)
    {
        _services = services;
    }

    public SetupValidationBuilder UseSqlServer<TContext>(string connectionString)
        where TContext : DbContext
    {
        _services.AddDbContext<TContext>(o => o.UseSqlServer(connectionString));
        _services.AddScoped<DbContext>(sp => sp.GetRequiredService<TContext>());
        _services.AddScoped<ISaveAuditRepository, EfCoreSaveAuditRepository>();
        return this;
    }

    public SetupValidationBuilder UseMongo(IMongoDatabase database)
    {
        _services.AddSingleton(database);
        _services.AddScoped<ISaveAuditRepository, MongoSaveAuditRepository>();
        return this;
    }

    public IServiceCollection Apply() => _services;
}
