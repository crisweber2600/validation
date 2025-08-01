using MassTransit;
using System.Linq;
using System.Linq.Expressions;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MongoDB.Driver;
using OpenTelemetry.Trace;
using Serilog;
using Validation.Domain.Validation;
using Validation.Infrastructure.Messaging;
using Validation.Infrastructure.Repositories;
using Validation.Infrastructure;
using Validation.Infrastructure.Reliability;
using Validation.Infrastructure.Metrics;
using Validation.Infrastructure.Auditing;
using Validation.Infrastructure.Observability;

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
        services.AddSingleton<IEnhancedManualValidatorService, EnhancedManualValidatorService>();

        // Add reliability services
        services.AddSingleton<DeleteReliabilityOptions>();
        services.AddScoped<DeletePipelineReliabilityPolicy>();
        services.AddScoped<ReliableMessagePublisher>();

        // Add metrics services
        services.AddSingleton<IMetricsCollector, MetricsCollector>();
        services.AddSingleton<MetricsOrchestratorOptions>();
        services.AddHostedService<MetricsOrchestrator>();

        // Add auditing services  
        services.AddScoped<NannyRecordAuditService>();
        services.AddSingleton<NannyRecordAuditOptions>();

        services.AddMassTransit(x =>
        {
            // Register the enhanced consumers
            x.AddConsumer<ReliableDeleteValidationConsumer<Validation.Domain.Entities.Item>>(typeof(ReliabilityConsumerDefinition<>));
            x.AddConsumer<ReliableDeleteValidationConsumer<Validation.Domain.Entities.NannyRecord>>(typeof(ReliabilityConsumerDefinition<>));
            
            configureBus?.Invoke(x);
        });

        services.AddLogging(loggingBuilder => loggingBuilder.AddSerilog());

        services.AddOpenTelemetry().WithTracing(builder => 
        {
            builder.AddAspNetCoreInstrumentation();
            builder.AddSource(ValidationObservability.ActivitySource.Name);
        });

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
            if (config.ManualRules != null && config.ManualRules.Count > 0)
            {
                var type = Type.GetType(config.Type, true)!;
                foreach (var rule in config.ManualRules)
                {
                    var param = Expression.Parameter(type, "x");
                    var prop = Expression.Property(param, rule.Property);
                    var propConv = Expression.Convert(prop, typeof(decimal));
                    var minConstant = Expression.Constant(rule.MinValue ?? 0m, typeof(decimal));
                    var body = Expression.GreaterThanOrEqual(propConv, minConstant);
                    var lambda = Expression.Lambda(body, param).Compile();
                    typeof(ServiceCollectionExtensions)
                        .GetMethod(nameof(AddValidatorRule))!
                        .MakeGenericMethod(type)
                        .Invoke(null, new object?[] { services, lambda });
                }
            }
        }

        // Set up MassTransit with dynamic consumer registration
        services.AddMassTransitTestHarness(x =>
        {
            foreach (var config in configs)
            {
                var type = Type.GetType(config.Type, true)!;
                if (config.SaveValidation)
                {
                    x.AddConsumer(typeof(SaveValidationConsumer<>).MakeGenericType(type));
                    services.AddScoped(typeof(SaveValidationConsumer<>).MakeGenericType(type));
                }
                if (config.SaveCommit)
                {
                    x.AddConsumer(typeof(SaveCommitConsumer<>).MakeGenericType(type));
                    services.AddScoped(typeof(SaveCommitConsumer<>).MakeGenericType(type));
                }
                if (config.DeleteValidation)
                {
                    x.AddConsumer(typeof(DeleteValidationConsumer<>).MakeGenericType(type));
                    services.AddScoped(typeof(DeleteValidationConsumer<>).MakeGenericType(type));
                }
                if (config.DeleteCommit)
                {
                    x.AddConsumer(typeof(DeleteCommitConsumer<>).MakeGenericType(type));
                    services.AddScoped(typeof(DeleteCommitConsumer<>).MakeGenericType(type));
                }
            }
            x.UsingInMemory((context, cfgBus) => cfgBus.ConfigureEndpoints(context));
        });

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
