using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using Validation.Domain.Validation;
using Validation.Infrastructure.Repositories;

namespace Validation.Infrastructure.Setup;

public class SetupValidationBuilder
{
    private readonly IServiceCollection _services;
    private readonly List<(Type Type, ValidationPlan Plan)> _plans = new();

    public SetupValidationBuilder(IServiceCollection services)
    {
        _services = services;
    }

    public SetupValidationBuilder UseSqlServer<TContext>(string connectionString) where TContext : DbContext
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

    public SetupValidationBuilder AddPlan<T>(ValidationPlan plan)
    {
        _plans.Add((typeof(T), plan));
        return this;
    }

    public IServiceCollection Apply()
    {
        _services.AddSingleton<IValidationPlanProvider>(sp =>
        {
            var provider = new InMemoryValidationPlanProvider();
            foreach (var (type, plan) in _plans)
            {
                typeof(InMemoryValidationPlanProvider)
                    .GetMethod("AddPlan")!
                    .MakeGenericMethod(type)
                    .Invoke(provider, new object[] { plan });
            }
            return provider;
        });

        _services.AddSingleton<IManualValidatorService, ManualValidatorService>();
        _services.AddSingleton<IEnhancedManualValidatorService, EnhancedManualValidatorService>();
        _services.AddScoped<SummarisationValidator>();

        return _services;
    }
}
