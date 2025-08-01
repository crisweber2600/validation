using Microsoft.Extensions.DependencyInjection;
using Validation.Infrastructure.Pipeline;

namespace Validation.Infrastructure.DI;

public static class MetricsPipelineServiceCollectionExtensions
{
    public static IServiceCollection AddMetricsPipeline(this IServiceCollection services)
    {
        services.AddSingleton<PipelineOrchestrator>();
        services.AddHostedService<PipelineWorker>();
        return services;
    }
}
