using Microsoft.Extensions.DependencyInjection;

namespace Validation.Infrastructure.Metrics;

public static class PipelineServiceCollectionExtensions
{
    public static IServiceCollection AddMetricsPipeline(this IServiceCollection services)
    {
        services.AddSingleton<PipelineWorkerOptions>();
        services.AddSingleton<PipelineOrchestrator>();
        services.AddHostedService<PipelineWorker>();
        return services;
    }
}
