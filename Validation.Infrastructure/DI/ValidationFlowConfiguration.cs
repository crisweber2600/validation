using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Validation.Infrastructure.Messaging;

namespace Validation.Infrastructure.DI;

public enum ThresholdType
{
    RawDifference,
    PercentChange
}

public class ValidationFlowDefinition
{
    public string Type { get; set; } = "";
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
        var opts = JsonSerializer.Deserialize<ValidationFlowOptions>(json);
        return opts ?? new ValidationFlowOptions();
    }
}

public static class ValidationFlowRegistrationExtensions
{
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

    public static IServiceCollection AddValidationFlows(this IServiceCollection services, ValidationFlowOptions opts)
    {
        foreach (var flow in opts.Flows)
        {
            var type = Type.GetType(flow.Type, throwOnError: true)!;
            if (flow.SaveValidation)
            {
                var m = typeof(ValidationFlowRegistrationExtensions).GetMethod(nameof(AddSaveValidation))!
                    .MakeGenericMethod(type);
                m.Invoke(null, new object[] { services });
            }
            if (flow.SaveCommit)
            {
                var m = typeof(ValidationFlowRegistrationExtensions).GetMethod(nameof(AddSaveCommit))!
                    .MakeGenericMethod(type);
                m.Invoke(null, new object[] { services });
            }
        }
        return services;
    }
}
