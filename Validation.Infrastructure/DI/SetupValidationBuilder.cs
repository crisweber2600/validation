using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using OpenTelemetry.Trace;
using Serilog;
using Validation.Domain.Validation;

namespace Validation.Infrastructure.DI;

public class SetupValidationBuilder
{
    private readonly IServiceCollection _services;
    private Action<IBusRegistrationConfigurator>? _configureBus;
    internal bool UseMongo { get; private set; }

    public SetupValidationBuilder(IServiceCollection services)
    {
        _services = services;
    }

    public void ConfigureBus(Action<IBusRegistrationConfigurator> configure)
    {
        _configureBus = configure;
    }

    public void SetupDatabase<TContext>(string connectionString) where TContext : DbContext
    {
        _services.AddDbContext<TContext>(o => o.UseInMemoryDatabase(connectionString));
        _services.AddScoped<DbContext>(sp => sp.GetRequiredService<TContext>());
    }

    public void SetupMongoDatabase(string connectionString, string dbName)
    {
        var client = new MongoClient(connectionString);
        var database = client.GetDatabase(dbName);
        _services.AddSingleton(database);
        UseMongo = true;
    }

    internal void Apply(IServiceCollection services)
    {
        services.AddSingleton<IValidationPlanProvider, InMemoryValidationPlanProvider>();
        services.AddSingleton<IManualValidatorService, ManualValidatorService>();
        services.AddMassTransit(x => { _configureBus?.Invoke(x); });
        services.AddLogging(b => b.AddSerilog());
        services.AddOpenTelemetry().WithTracing(b => b.AddAspNetCoreInstrumentation());
    }
}
