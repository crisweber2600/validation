using Microsoft.Extensions.DependencyInjection;

namespace Validation.Infrastructure.DI;

public static class MetricsPipelineServiceCollectionExtensions
{
    public static IServiceCollection AddMetricsPipeline(this IServiceCollection services)
    {
        services.AddSingleton<ISummarisationService, AverageSummarisationService>();
        services.AddSingleton<PipelineOrchestrator>();
        services.AddSingleton<PipelineWorkerOptions>();
        services.AddHostedService<PipelineWorker>();
        return services;
    }
}
