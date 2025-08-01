using MassTransit;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using OpenTelemetry.Trace;
using Serilog;
using Validation.Domain.Validation;
using Validation.Infrastructure.Messaging;
using Validation.Infrastructure.Repositories;
using Validation.Infrastructure;

namespace Validation.Infrastructure.DI;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddValidationInfrastructure(
        this IServiceCollection services,
        Action<IBusRegistrationConfigurator>? configureBus = null)
    {
        services.AddScoped<ISaveAuditRepository, EfCoreSaveAuditRepository>();
        services.AddSingleton<IValidationPlanProvider, InMemoryValidationPlanProvider>();
        services.AddSingleton<IManualValidatorService, ManualValidatorService>();

        services.AddMassTransit(x =>
        {
            configureBus?.Invoke(x);
        });

        services.AddLogging(loggingBuilder => loggingBuilder.AddSerilog());

        services.AddOpenTelemetry().WithTracing(builder => builder.AddAspNetCoreInstrumentation());

        return services;
    }

    public static IServiceCollection AddMongoValidationInfrastructure(
        this IServiceCollection services, IMongoDatabase database,
        Action<IBusRegistrationConfigurator>? configureBus = null)
    {
        services.AddSingleton(database);
        services.AddScoped<ISaveAuditRepository, MongoSaveAuditRepository>();
        services.AddSingleton<IValidationPlanProvider, InMemoryValidationPlanProvider>();
        services.AddSingleton<IManualValidatorService, ManualValidatorService>();

        services.AddMassTransit(x =>
        {
            configureBus?.Invoke(x);
        });

        services.AddLogging(loggingBuilder => loggingBuilder.AddSerilog());

        services.AddOpenTelemetry().WithTracing(builder => builder.AddAspNetCoreInstrumentation());

        return services;
    }

    public static IServiceCollection AddValidatorRule<T>(this IServiceCollection services, Func<T, bool> rule)
    {
        var descriptor = services.FirstOrDefault(d => d.ServiceType == typeof(IManualValidatorService));
        ManualValidatorService svc;
        if (descriptor?.ImplementationInstance is ManualValidatorService existing)
        {
            svc = existing;
        }
        else
        {
            svc = new ManualValidatorService();
            services.AddSingleton<IManualValidatorService>(svc);
        }
        svc.AddRule(rule);
        return services;
    }

    public static IServiceCollection AddValidatorService(this IServiceCollection services)
    {
        services.AddSingleton<IManualValidatorService, ManualValidatorService>();
        return services;
    }

    public static IServiceCollection AddSaveValidation<T>(this IServiceCollection services)
    {
        services.AddMassTransitTestHarness(x =>
        {
            x.AddConsumer<SaveValidationConsumer<T>>();
            x.UsingInMemory((context, cfg) => cfg.ConfigureEndpoints(context));
        });
        services.AddScoped<SaveValidationConsumer<T>>();
        return services;
    }

    public static IServiceCollection AddSaveCommit<T>(this IServiceCollection services)
    {
        services.AddMassTransitTestHarness(x =>
        {
            x.AddConsumer<SaveCommitConsumer<T>>();
            x.UsingInMemory((context, cfg) => cfg.ConfigureEndpoints(context));
        });
        services.AddScoped<SaveCommitConsumer<T>>();
        return services;
    }

    public static IServiceCollection AddDeleteValidation<T>(this IServiceCollection services)
    {
        services.AddMassTransitTestHarness(x =>
        {
            x.AddConsumer<DeleteValidationConsumer<T>>();
            x.UsingInMemory((context, cfg) => cfg.ConfigureEndpoints(context));
        });
        services.AddScoped<DeleteValidationConsumer<T>>();
        return services;
    }

    public static IServiceCollection AddDeleteCommit<T>(this IServiceCollection services)
    {
        services.AddMassTransitTestHarness(x =>
        {
            x.AddConsumer<DeleteCommitConsumer<T>>();
            x.UsingInMemory((context, cfg) => cfg.ConfigureEndpoints(context));
        });
        services.AddScoped<DeleteCommitConsumer<T>>();
        return services;
    }

    public static IServiceCollection AddValidationFlows(this IServiceCollection services, IEnumerable<ValidationFlowConfig> configs)
    {
        // Set up validation plan provider with configurations
        services.AddSingleton<IValidationPlanProvider>(serviceProvider =>
        {
            var provider = new InMemoryValidationPlanProvider();
            foreach (var config in configs)
            {
                if (!string.IsNullOrEmpty(config.MetricProperty) && config.ThresholdType.HasValue && config.ThresholdValue.HasValue)
                {
                    var type = Type.GetType(config.Type, true)!;
                    var param = Expression.Parameter(typeof(object), "o");
                    var cast = Expression.Convert(param, type);
                    var prop = Expression.Property(cast, config.MetricProperty);
                    var conv = Expression.Convert(prop, typeof(decimal));
                    var lambda = Expression.Lambda<Func<object, decimal>>(conv, param).Compile();
                    var plan = new ValidationPlan(lambda, config.ThresholdType.Value, config.ThresholdValue.Value);
                    
                    typeof(InMemoryValidationPlanProvider).GetMethod("AddPlan")!
                        .MakeGenericMethod(type)
                        .Invoke(provider, new object[] { plan });
                }
            }
            return provider;
        });

        // Register dependencies for consumers
        services.AddScoped<ISaveAuditRepository, EfCoreSaveAuditRepository>();
        services.AddSingleton<IManualValidatorService, ManualValidatorService>();
        services.AddScoped<SummarisationValidator>();

        foreach (var config in configs)
        {
            var type = Type.GetType(config.Type, true)!;
            if (config.SaveValidation)
            {
                typeof(ServiceCollectionExtensions)
                    .GetMethod(nameof(AddSaveValidation))!
                    .MakeGenericMethod(type)
                    .Invoke(null, new object[] { services });
            }
            if (config.SaveCommit)
            {
                typeof(ServiceCollectionExtensions)
                    .GetMethod(nameof(AddSaveCommit))!
                    .MakeGenericMethod(type)
                    .Invoke(null, new object[] { services });
            }
            if (config.DeleteValidation)
            {
                typeof(ServiceCollectionExtensions)
                    .GetMethod(nameof(AddDeleteValidation))!
                    .MakeGenericMethod(type)
                    .Invoke(null, new object[] { services });
            }
            if (config.DeleteCommit)
            {
                typeof(ServiceCollectionExtensions)
                    .GetMethod(nameof(AddDeleteCommit))!
                    .MakeGenericMethod(type)
                    .Invoke(null, new object[] { services });
            }
        }

        services.AddLogging(b => b.AddSerilog());
        services.AddOpenTelemetry().WithTracing(b => b.AddAspNetCoreInstrumentation());

        return services;
    }
}

public static class ValidationFlowServiceCollectionExtensions
{
    public class ValidationFlowOptions
    {
        public IServiceCollection Services { get; }
        public ValidationFlowOptions(IServiceCollection services)
        {
            Services = services;
        }
    }

    public static IServiceCollection SetupDatabase<TContext>(this ValidationFlowOptions options, string connectionString)
        where TContext : DbContext
    {
        options.Services.AddDbContext<TContext>(o => o.UseInMemoryDatabase(connectionString));
        options.Services.AddScoped<DbContext>(sp => sp.GetRequiredService<TContext>());
        options.Services.AddScoped<ISaveAuditRepository, EfCoreSaveAuditRepository>();
        return options.Services;
    }

    public static IServiceCollection SetupMongoDatabase(this ValidationFlowOptions options, string connectionString, string dbName)
    {
        var client = new MongoClient(connectionString);
        var database = client.GetDatabase(dbName);
        options.Services.AddSingleton(database);
        options.Services.AddScoped<ISaveAuditRepository, MongoSaveAuditRepository>();
        return options.Services;
    }

    public static IServiceCollection AddValidationFlow<TRule>(this IServiceCollection services, Action<ValidationFlowOptions>? configure = null)
        where TRule : class, IValidationRule
    {
        services.AddScoped<IValidationRule, TRule>();
        services.AddSingleton<IValidationPlanProvider, InMemoryValidationPlanProvider>();
        services.AddSingleton<IManualValidatorService, ManualValidatorService>();
        services.AddScoped<SummarisationValidator>();
        services.AddMassTransitTestHarness(x =>
        {
            x.AddConsumer<SaveRequestedConsumer>();
            x.UsingInMemory((context, cfg) => cfg.ConfigureEndpoints(context));
        });
        services.AddLogging(b => b.AddSerilog());
        services.AddOpenTelemetry().WithTracing(b => b.AddAspNetCoreInstrumentation());
        var options = new ValidationFlowOptions(services);
        configure?.Invoke(options);
        return services;
    }
}
