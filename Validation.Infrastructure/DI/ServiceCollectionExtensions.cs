using MassTransit;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MongoDB.Driver;
using OpenTelemetry.Trace;
using Serilog;
using Validation.Domain.Validation;
using Validation.Domain.Events;
using Validation.Domain.Repositories;
using Validation.Domain.Providers;
using Validation.Infrastructure.Messaging;
using Validation.Infrastructure.Repositories;
using Validation.Infrastructure;
using Validation.Infrastructure.Reliability;
using Validation.Infrastructure.Metrics;
using Validation.Infrastructure.Auditing;
using Validation.Infrastructure.Observability;
using Validation.Infrastructure.Pipeline;

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

        // Add unified event hub
        services.AddSingleton<IValidationEventHub, Domain.Events.ValidationEventHub>();

        // Add providers for unified validation system
        services.AddSingleton<IEntityIdProvider, ReflectionEntityIdProvider>();
        services.AddSingleton<IApplicationNameProvider, ConfigurationApplicationNameProvider>();
        services.AddSingleton<ISequenceValidator, SequenceValidator>();

        // Add pipeline orchestrators
        services.AddScoped<IPipelineOrchestrator<MetricsInput>, MetricsPipelineOrchestrator>();
        services.AddScoped<IPipelineOrchestrator<SummarisationInput>, SummarisationPipelineOrchestrator>();
        services.AddScoped<ValidationFlowOrchestrator>();

        // Add repository services (generic registration)
        services.AddScoped(typeof(ISummaryRecordRepository<>), typeof(EfCoreSummaryRecordRepository<>));

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

        // Add batch processing options
        services.AddSingleton<BatchProcessingOptions>();

        services.AddMassTransit(x =>
        {
            // Register the enhanced consumers without generic definition types to avoid MassTransit issues
            x.AddConsumer<ReliableDeleteValidationConsumer<Validation.Domain.Entities.Item>>();
            x.AddConsumer<ReliableDeleteValidationConsumer<Validation.Domain.Entities.NannyRecord>>();
            x.AddConsumer<GenericSaveValidationConsumer>();
            
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

        // Add unified event hub
        services.AddSingleton<IValidationEventHub, Domain.Events.ValidationEventHub>();

        // Add providers for unified validation system
        services.AddSingleton<IEntityIdProvider, ReflectionEntityIdProvider>();
        services.AddSingleton<IApplicationNameProvider, EnvironmentApplicationNameProvider>();
        services.AddSingleton<ISequenceValidator, SequenceValidator>();

        // Add pipeline orchestrators
        services.AddScoped<IPipelineOrchestrator<MetricsInput>, MetricsPipelineOrchestrator>();
        services.AddScoped<IPipelineOrchestrator<SummarisationInput>, SummarisationPipelineOrchestrator>();
        services.AddScoped<ValidationFlowOrchestrator>();

        // Add MongoDB repository services
        services.AddScoped(typeof(ISummaryRecordRepository<>), typeof(MongoSummaryRecordRepository<>));

        services.AddMassTransit(x =>
        {
            x.AddConsumer<GenericSaveValidationConsumer>();
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
        
        // Register entity ID and application name providers if not already registered
        if (!services.Any(s => s.ServiceType == typeof(IEntityIdProvider)))
        {
            services.AddSingleton<IEntityIdProvider, ReflectionEntityIdProvider>();
        }
        if (!services.Any(s => s.ServiceType == typeof(IApplicationNameProvider)))
        {
            services.AddSingleton<IApplicationNameProvider>(sp => new StaticApplicationNameProvider("ValidationFlowApp"));
        }

        // Register flow configs for ValidationFlowOrchestrator
        services.AddSingleton<IEnumerable<ValidationFlowConfig>>(configs);

        // Set up MassTransit with dynamic consumer registration
        services.AddMassTransitTestHarness(x =>
        {
            // Add generic consumer for all flows
            x.AddConsumer<GenericSaveValidationConsumer>();
            
            foreach (var config in configs)
            {
                var type = Type.GetType(config.Type, true)!;
                if (config.SaveValidation)
                {
                    x.AddConsumer(typeof(SaveValidationConsumer<>).MakeGenericType(type));
                    services.AddScoped(typeof(SaveValidationConsumer<>).MakeGenericType(type));
                    
                    // Add entity-aware consumer to support WithEntityIdSelector pattern
                    x.AddConsumer(typeof(EntityAwareSaveValidationConsumer<>).MakeGenericType(type));
                    services.AddScoped(typeof(EntityAwareSaveValidationConsumer<>).MakeGenericType(type));
                    
                    // Add batch consumer if enabled
                    x.AddConsumer(typeof(BatchSaveValidationConsumer<>).MakeGenericType(type));
                    services.AddScoped(typeof(BatchSaveValidationConsumer<>).MakeGenericType(type));
                }
                if (config.SaveCommit)
                {
                    x.AddConsumer(typeof(SaveCommitConsumer<>).MakeGenericType(type));
                    services.AddScoped(typeof(SaveCommitConsumer<>).MakeGenericType(type));
                }
                if (config.DeleteValidation)
                {
                    if (config.SoftDeleteSupport)
                    {
                        // Use reliable delete validation consumer for soft deletes
                        x.AddConsumer(typeof(ReliableDeleteValidationConsumer<>).MakeGenericType(type));
                        services.AddScoped(typeof(ReliableDeleteValidationConsumer<>).MakeGenericType(type));
                    }
                    else
                    {
                        x.AddConsumer(typeof(DeleteValidationConsumer<>).MakeGenericType(type));
                        services.AddScoped(typeof(DeleteValidationConsumer<>).MakeGenericType(type));
                    }
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

    /// <summary>
    /// Add configurable entity ID provider with optional configuration
    /// </summary>
    public static IServiceCollection AddConfigurableEntityIdProvider(
        this IServiceCollection services, Action<ConfigurableEntityIdProvider>? configure = null)
    {
        var provider = new ConfigurableEntityIdProvider();
        configure?.Invoke(provider);
        services.AddSingleton<IEntityIdProvider>(provider);
        return services;
    }

    /// <summary>
    /// Add reflection-based entity ID provider with property priority
    /// </summary>
    public static IServiceCollection AddReflectionBasedEntityIdProvider(
        this IServiceCollection services, params string[] propertyPriority)
    {
        services.AddSingleton<IEntityIdProvider>(
            sp => new ReflectionBasedEntityIdProvider(propertyPriority));
        return services;
    }

    /// <summary>
    /// Configure entity ID selector for a specific type (WithEntityIdSelector pattern)
    /// </summary>
    public static IServiceCollection WithEntityIdSelector<T>(
        this IServiceCollection services, Func<T, string> selector)
    {
        // Remove any existing IEntityIdProvider
        var toRemove = services.FirstOrDefault(d => d.ServiceType == typeof(IEntityIdProvider));
        if (toRemove != null) services.Remove(toRemove);

        services.AddConfigurableEntityIdProvider(provider => provider.RegisterSelector(selector));
        
        // Register the entity-aware consumer for this type
        services.AddScoped<EntityAwareSaveValidationConsumer<T>>();
        
        return services;
    }
}
