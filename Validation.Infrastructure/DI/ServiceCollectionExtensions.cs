using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using OpenTelemetry.Trace;
using Serilog;
using Validation.Domain.Validation;
using Validation.Infrastructure.Messaging;
using Validation.Infrastructure.Repositories;

namespace Validation.Infrastructure.DI;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddValidationInfrastructure(
        this IServiceCollection services,
        Action<IBusRegistrationConfigurator>? configureBus = null)
    {
        services.AddScoped<ISaveAuditRepository, EfCoreSaveAuditRepository>();

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

        services.AddMassTransit(x =>
        {
            configureBus?.Invoke(x);
        });

        services.AddLogging(loggingBuilder => loggingBuilder.AddSerilog());

        services.AddOpenTelemetry().WithTracing(builder => builder.AddAspNetCoreInstrumentation());

        return services;
    }
}

public static class ValidationFlowServiceCollectionExtensions
{
    public class ValidationFlowSetupOptions
    {
        public IServiceCollection Services { get; }
        public ValidationFlowSetupOptions(IServiceCollection services)
        {
            Services = services;
        }
    }

    public static IServiceCollection SetupDatabase<TContext>(this ValidationFlowSetupOptions options, string connectionString)
        where TContext : DbContext
    {
        options.Services.AddDbContext<TContext>(o => o.UseInMemoryDatabase(connectionString));
        options.Services.AddScoped<DbContext>(sp => sp.GetRequiredService<TContext>());
        options.Services.AddScoped<ISaveAuditRepository, EfCoreSaveAuditRepository>();
        return options.Services;
    }

    public static IServiceCollection SetupMongoDatabase(this ValidationFlowSetupOptions options, string connectionString, string dbName)
    {
        var client = new MongoClient(connectionString);
        var database = client.GetDatabase(dbName);
        options.Services.AddSingleton(database);
        options.Services.AddScoped<ISaveAuditRepository, MongoSaveAuditRepository>();
        return options.Services;
    }

    public static IServiceCollection AddValidationFlow<TRule>(this IServiceCollection services, Action<ValidationFlowSetupOptions>? configure = null)
        where TRule : class, IValidationRule
    {
        services.AddScoped<IValidationRule, TRule>();
        services.AddScoped<SummarisationValidator>();
        services.AddMassTransitTestHarness(x =>
        {
            x.AddConsumer<SaveRequestedConsumer>();
            x.UsingInMemory((context, cfg) => cfg.ConfigureEndpoints(context));
        });
        services.AddLogging(b => b.AddSerilog());
        services.AddOpenTelemetry().WithTracing(b => b.AddAspNetCoreInstrumentation());
        var options = new ValidationFlowSetupOptions(services);
        configure?.Invoke(options);
        return services;
    }

    public static IServiceCollection AddSaveValidation<T>(this IServiceCollection services)
    {
        services.AddScoped<SaveValidationConsumer<T>>();
        return services;
    }

    public static IServiceCollection AddSaveCommit<T>(this IServiceCollection services)
    {
        services.AddScoped<SaveCommitConsumer<T>>();
        return services;
    }

    public static IServiceCollection AddValidationFlows(this IServiceCollection services, ValidationFlowOptions options)
    {
        foreach (var flow in options.Flows)
        {
            var type = Type.GetType(flow.Type) ?? throw new InvalidOperationException($"Type {flow.Type} not found");
            if (flow.SaveValidation)
            {
                var method = typeof(ServiceCollectionExtensions).GetMethod(nameof(AddSaveValidation))!.MakeGenericMethod(type);
                method.Invoke(null, new object[] { services });
            }
            if (flow.SaveCommit)
            {
                var method = typeof(ServiceCollectionExtensions).GetMethod(nameof(AddSaveCommit))!.MakeGenericMethod(type);
                method.Invoke(null, new object[] { services });
            }
        }
        return services;
    }
}
