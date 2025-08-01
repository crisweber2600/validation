using Microsoft.Extensions.DependencyInjection;

namespace Validation.Infrastructure.Metrics;

public static class MetricsPipelineExtensions
{
    public static IServiceCollection AddMetricsPipeline(this IServiceCollection services, Action<MetricsPipelineOptions>? configure = null)
    {
        var options = new MetricsPipelineOptions();
        configure?.Invoke(options);

        services.AddSingleton(options);
        services.AddSingleton<PipelineOrchestrator>();
        services.AddHostedService<PipelineWorker>();

        return services;
    }
}
