using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using MassTransit;
using Validation.Infrastructure.Messaging;
using Validation.Domain.Validation;
using Validation.Infrastructure.Repositories;

namespace Validation.Infrastructure.DI;

public enum ThresholdType
{
    Raw,
    Percent
}

public class ValidationFlowDefinition
{
    public string Type { get; set; } = string.Empty;
    public bool SaveValidation { get; set; }
    public bool SaveCommit { get; set; }
    public string? MetricProperty { get; set; }
    public ThresholdType ThresholdType { get; set; }
    public decimal ThresholdValue { get; set; }
}

public class ValidationFlowOptions
{
    public List<ValidationFlowDefinition> Flows { get; set; } = new();

    public static ValidationFlowOptions Load(string json)
    {
        var opts = new JsonSerializerOptions();
        opts.Converters.Add(new JsonStringEnumConverter());
        return JsonSerializer.Deserialize<ValidationFlowOptions>(json, opts) ?? new ValidationFlowOptions();
    }
}

public static class ValidationFlowRegistrationExtensions
{
    public static IServiceCollection AddSaveValidation<T>(this IServiceCollection services)
    {
        services.AddScoped<SaveValidationConsumer<T>>();
        services.AddScoped<SummarisationValidator>();
        services.AddMassTransitTestHarness(cfg => cfg.AddConsumer<SaveValidationConsumer<T>>());
        services.AddLogging(b => b.AddSerilog());
        return services;
    }

    public static IServiceCollection AddSaveCommit<T>(this IServiceCollection services)
    {
        services.AddScoped<SaveCommitConsumer<T>>();
        services.AddMassTransitTestHarness(cfg => cfg.AddConsumer<SaveCommitConsumer<T>>());
        services.AddLogging(b => b.AddSerilog());
        return services;
    }

    public static IServiceCollection AddValidationFlows(this IServiceCollection services, ValidationFlowOptions opts)
    {
        foreach (var flow in opts.Flows)
        {
            var type = Type.GetType(flow.Type) ?? throw new InvalidOperationException($"Type {flow.Type} not found");
            if (flow.SaveValidation)
            {
                var method = typeof(ValidationFlowRegistrationExtensions).GetMethod(nameof(AddSaveValidation))!.MakeGenericMethod(type);
                method.Invoke(null, new object[] { services });
            }
            if (flow.SaveCommit)
            {
                var method = typeof(ValidationFlowRegistrationExtensions).GetMethod(nameof(AddSaveCommit))!.MakeGenericMethod(type);
                method.Invoke(null, new object[] { services });
            }
        }
        return services;
    }
}

