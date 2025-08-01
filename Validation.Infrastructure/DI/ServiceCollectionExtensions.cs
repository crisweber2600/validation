using MassTransit;
using System.Linq;
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

    public static IServiceCollection AddValidationFlows(this IServiceCollection services, IEnumerable<ValidationFlowConfig> configs)
    {
        services.AddSingleton<IValidationPlanProvider>(sp =>
        {
            var provider = new InMemoryValidationPlanProvider();
            foreach (var cfg in configs)
            {
                var type = Type.GetType(cfg.Type) ?? throw new ArgumentException($"Type {cfg.Type} not found");
                var prop = type.GetProperty(cfg.MetricProperty);
                if (prop == null)
                    throw new ArgumentException($"Property {cfg.MetricProperty} not found on {type}");

                var param = System.Linq.Expressions.Expression.Parameter(typeof(object), "o");
                var body = System.Linq.Expressions.Expression.Convert(
                    System.Linq.Expressions.Expression.Property(
                        System.Linq.Expressions.Expression.Convert(param, type), prop), typeof(decimal));
                var selector = System.Linq.Expressions.Expression.Lambda<Func<object, decimal>>(body, param).Compile();
                var plan = new ValidationPlan(selector, cfg.ThresholdType, cfg.ThresholdValue);
                var method = typeof(InMemoryValidationPlanProvider).GetMethod(nameof(InMemoryValidationPlanProvider.AddPlan))!.MakeGenericMethod(type);
                method.Invoke(provider, new object[] { plan });
            }
            return provider;
        });
        services.AddScoped<SummarisationValidator>();
        services.AddMassTransitTestHarness(x =>
        {
            foreach (var cfg in configs)
            {
                var type = Type.GetType(cfg.Type) ?? throw new ArgumentException($"Type {cfg.Type} not found");
                if (cfg.SaveValidation)
                {
                    x.AddConsumer(typeof(SaveValidationConsumer<>).MakeGenericType(type));
                    services.AddScoped(typeof(SaveValidationConsumer<>).MakeGenericType(type));
                }
                if (cfg.SaveCommit)
                {
                    x.AddConsumer(typeof(SaveCommitConsumer<>).MakeGenericType(type));
                    services.AddScoped(typeof(SaveCommitConsumer<>).MakeGenericType(type));
                }
            }
            x.UsingInMemory((context, cfgBus) => cfgBus.ConfigureEndpoints(context));
        });
        services.AddLogging(b => b.AddSerilog());
        services.AddOpenTelemetry().WithTracing(b => b.AddAspNetCoreInstrumentation());
        return services;
    }
}
