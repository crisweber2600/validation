using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using Validation.Domain.Validation;
using Validation.Infrastructure.Repositories;

namespace Validation.Infrastructure.DI;

public class SetupValidationBuilder
{
    private Action<IServiceCollection>? _dbConfig;
    internal bool UseMongo { get; private set; }

    public void SetupDatabase<TContext>(string connectionString) where TContext : DbContext
    {
        UseMongo = false;
        _dbConfig = services =>
        {
            services.AddDbContext<TContext>(o => o.UseInMemoryDatabase(connectionString));
            services.AddScoped<DbContext>(sp => sp.GetRequiredService<TContext>());
        };
    }

    public void SetupMongoDatabase(IMongoDatabase database)
    {
        UseMongo = true;
        _dbConfig = services =>
        {
            services.AddSingleton(database);
        };
    }

    internal void Apply(IServiceCollection services)
    {
        _dbConfig?.Invoke(services);

        // register common services
        var provider = new InMemoryValidationPlanProvider();
        services.AddSingleton<IValidationPlanProvider>(provider);
        services.AddSingleton<IManualValidatorService, ManualValidatorService>();
        services.AddScoped<SummarisationValidator>();

        if (UseMongo)
        {
            services.AddScoped<ISaveAuditRepository, MongoSaveAuditRepository>();
        }
        else
        {
            services.AddScoped<ISaveAuditRepository, EfCoreSaveAuditRepository>();
        }
    }
}

public static class SetupValidationExtensions
{
    public static IServiceCollection SetupValidation(this IServiceCollection services, Action<SetupValidationBuilder> configure)
    {
        var builder = new SetupValidationBuilder();
        configure(builder);
        builder.Apply(services);
        return services;
    }

    public static IServiceCollection AddSaveValidation<T>(this IServiceCollection services, Func<T, decimal> metricSelector, ThresholdType thresholdType, decimal thresholdValue) where T : class
    {
        var existing = services.FirstOrDefault(d => d.ServiceType == typeof(IValidationPlanProvider));
        services.Remove(existing);
        var provider = new InMemoryValidationPlanProvider();
        provider.AddPlan<T>(new ValidationPlan(o => metricSelector((T)o), thresholdType, thresholdValue));
        services.AddSingleton<IValidationPlanProvider>(provider);
        services.AddScoped<SummarisationValidator>();
        return services;
    }

    public static IServiceCollection AddSetupValidation<T>(this IServiceCollection services, Action<SetupValidationBuilder> configure, Func<T, decimal> metricSelector = null, ThresholdType thresholdType = ThresholdType.PercentChange, decimal thresholdValue = 0.1m) where T : class
    {
        services.SetupValidation(configure);
        if (metricSelector != null)
        {
            services.AddSaveValidation(metricSelector, thresholdType, thresholdValue);
        }
        return services;
    }
}
