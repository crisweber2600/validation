using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using MassTransit;
using Validation.Domain.Validation;
using Validation.Infrastructure.Messaging;
using Validation.Infrastructure.Repositories;

namespace Validation.Infrastructure.DI;

public static class SetupValidationExtensions
{
    public static IServiceCollection SetupValidation(this IServiceCollection services, Action<SetupValidationBuilder> configure)
    {
        var builder = new SetupValidationBuilder(services);
        configure(builder);
        builder.Apply(services);
        return services;
    }

    public static IServiceCollection AddSaveValidation<T>(this IServiceCollection services,
        Func<T, decimal>? metricSelector = null,
        ThresholdType thresholdType = ThresholdType.PercentChange,
        decimal thresholdValue = 0.1m) where T : class
    {
        services.RemoveAll<IValidationPlanProvider>();
        var provider = new InMemoryValidationPlanProvider();
        if (metricSelector != null)
        {
            Func<object, decimal> selector = o => metricSelector((T)o);
            var plan = new ValidationPlan(selector, thresholdType, thresholdValue);
            provider.AddPlan<T>(plan);
        }
        else
        {
            provider.AddPlan<T>(new ValidationPlan(Array.Empty<IValidationRule>()));
        }
        services.AddSingleton<IValidationPlanProvider>(provider);
        services.AddScoped<SummarisationValidator>();
        services.AddMassTransitTestHarness(x =>
        {
            x.AddConsumer<SaveValidationConsumer<T>>();
            x.AddConsumer<SaveCommitConsumer<T>>();
            x.UsingInMemory((context, cfg) => cfg.ConfigureEndpoints(context));
        });
        return services;
    }

    public static IServiceCollection AddSetupValidation<T>(this IServiceCollection services,
        Action<SetupValidationBuilder> configure,
        Func<T, decimal>? metricSelector = null,
        ThresholdType thresholdType = ThresholdType.PercentChange,
        decimal thresholdValue = 0.1m) where T : class
    {
        var capture = new SetupValidationBuilder(new ServiceCollection());
        configure(capture);
        services.SetupValidation(configure);

        if (capture.UseMongo)
            services.AddScoped<ISaveAuditRepository, MongoSaveAuditRepository>();
        else
            services.AddScoped<ISaveAuditRepository, EfCoreSaveAuditRepository>();

        services.AddSaveValidation(metricSelector, thresholdType, thresholdValue);
        return services;
    }
}
