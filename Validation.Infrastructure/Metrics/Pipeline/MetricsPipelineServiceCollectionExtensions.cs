using Microsoft.Extensions.DependencyInjection;

namespace Validation.Infrastructure.Metrics.Pipeline;

public static class MetricsPipelineServiceCollectionExtensions
{
    public static IServiceCollection AddMetricsPipeline(this IServiceCollection services, Action<MetricsPipelineOptions>? configure = null)
    {
        var options = new MetricsPipelineOptions();
        configure?.Invoke(options);
        services.AddSingleton(options);
        services.AddSingleton<IMetricGatherer, InMemoryMetricGatherer>();
        services.AddSingleton<ISummarisationService, InMemorySummarisationService>();
        services.AddSingleton<PipelineOrchestrator>();
        services.AddHostedService<PipelineWorker>();
        return services;
    }
}
