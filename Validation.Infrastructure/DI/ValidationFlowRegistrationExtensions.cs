using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Validation.Infrastructure.Messaging;

namespace Validation.Infrastructure.DI;

public static class ValidationFlowRegistrationExtensions
{
    public static void AddSaveValidation<T>(this IServiceCollection services, IBusRegistrationConfigurator cfg)
    {
        cfg.AddConsumer<SaveValidationConsumer<T>>();
        services.AddScoped<SaveValidationConsumer<T>>();
    }

    public static void AddSaveCommit<T>(this IServiceCollection services, IBusRegistrationConfigurator cfg)
    {
        cfg.AddConsumer<SaveCommitConsumer<T>>();
        services.AddScoped<SaveCommitConsumer<T>>();
    }

    public static void AddDeleteValidation<T>(this IServiceCollection services, IBusRegistrationConfigurator cfg)
    {
        cfg.AddConsumer<DeleteValidationConsumer<T>>();
        services.AddScoped<DeleteValidationConsumer<T>>();
    }

    public static void AddDeleteCommit<T>(this IServiceCollection services, IBusRegistrationConfigurator cfg) where T : class
    {
        cfg.AddConsumer<DeleteCommitConsumer<T>>();
        services.AddScoped<DeleteCommitConsumer<T>>();
    }
}
