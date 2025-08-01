using Microsoft.Extensions.DependencyInjection;
using Validation.Domain.Validation;
using Validation.Infrastructure.Repositories;

namespace Validation.Infrastructure.DI;

public static class SetupValidationExtensions
{
    public static IServiceCollection SetupValidation(this IServiceCollection services, Action<SetupValidationBuilder> configure)
    {
        var builder = new SetupValidationBuilder();
        configure(builder);
        builder.Apply(services);
        return services;
    }

    public static IServiceCollection AddSaveValidation<T>(this IServiceCollection services,
        Func<T, decimal>? metricSelector = null,
        ThresholdType thresholdType = ThresholdType.PercentChange,
        decimal thresholdValue = 0.1m) where T : class
    {
        services.AddSingleton<IValidationPlanProvider>(sp =>
        {
            var provider = new InMemoryValidationPlanProvider();
            Func<object, decimal> selector = metricSelector != null ? o => metricSelector((T)o) : _ => 0m;
            var plan = new ValidationPlan(selector, thresholdType, thresholdValue);
            provider.AddPlan<T>(plan);
            return provider;
        });
        return services;
    }

    public static IServiceCollection AddSetupValidation<T>(this IServiceCollection services,
        Action<SetupValidationBuilder> configure,
        Func<T, decimal>? metricSelector = null,
        ThresholdType thresholdType = ThresholdType.PercentChange,
        decimal thresholdValue = 0.1m) where T : class
    {
        var builder = new SetupValidationBuilder();
        configure(builder);
        services.SetupValidation(configure);
        services.AddSaveValidation<T>(metricSelector, thresholdType, thresholdValue);
        if (builder.UseMongoDatabase)
            services.AddScoped<ISaveAuditRepository, MongoSaveAuditRepository>();
        else
            services.AddScoped<ISaveAuditRepository, EfCoreSaveAuditRepository>();
        return services;
    }
}
