using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using Validation.Domain.Validation;
using Validation.Infrastructure.Repositories;

namespace Validation.Infrastructure.DI;

public class SetupValidationBuilder
{
    private Action<IServiceCollection>? _configure;

    public bool UseMongoDatabase { get; private set; }

    public void UseEntityFramework<TContext>(string connectionString) where TContext : DbContext
    {
        _configure += services =>
        {
            services.AddDbContext<TContext>(o => o.UseInMemoryDatabase(connectionString));
            services.AddScoped<DbContext>(sp => sp.GetRequiredService<TContext>());
        };
    }

    public void UseMongo(string connectionString, string dbName)
    {
        UseMongoDatabase = true;
        _configure += services =>
        {
            var client = new MongoClient(connectionString);
            var database = client.GetDatabase(dbName);
            services.AddSingleton(database);
        };
    }

    public IServiceCollection Apply(IServiceCollection services)
    {
        _configure?.Invoke(services);
        services.AddSingleton<IValidationPlanProvider, InMemoryValidationPlanProvider>();
        services.AddScoped<SummarisationValidator>();
        return services;
    }
}
