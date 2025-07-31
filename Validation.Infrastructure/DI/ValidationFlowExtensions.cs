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

public static class ValidationFlowExtensions
{
    public static IServiceCollection AddValidationFlow<TRule>(
        this IServiceCollection services,
        Action<ValidationFlowBuilder>? configure = null)
        where TRule : class, IValidationRule
    {
        var builder = new ValidationFlowBuilder(services);
        configure?.Invoke(builder);

        services.AddMassTransit(x =>
        {
            x.AddConsumer<SaveRequestedConsumer>();
            x.UsingInMemory((context, cfg) => cfg.ConfigureEndpoints(context));
        });

        services.AddLogging(b => b.AddSerilog());
        services.AddOpenTelemetry().WithTracing(b => b.AddAspNetCoreInstrumentation());

        return services;
    }

    public static ValidationFlowBuilder SetupDatabase<TContext>(
        this ValidationFlowBuilder builder, string connectionString)
        where TContext : DbContext
    {
        builder.Services.AddDbContext<TContext>(o => o.UseInMemoryDatabase(connectionString));
        builder.Services.AddScoped<DbContext>(sp => sp.GetRequiredService<TContext>());
        builder.Services.AddScoped<ISaveAuditRepository, EfCoreSaveAuditRepository>();
        return builder;
    }

    public static ValidationFlowBuilder SetupMongoDatabase(
        this ValidationFlowBuilder builder, string connectionString, string dbName)
    {
        var client = new MongoClient(connectionString);
        builder.Services.AddSingleton<IMongoClient>(client);
        builder.Services.AddSingleton(client.GetDatabase(dbName));
        builder.Services.AddScoped<ISaveAuditRepository, MongoSaveAuditRepository>();
        return builder;
    }
}
