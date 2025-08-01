using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Validation.Infrastructure.Metrics;

public static class MetricsPipelineExtensions
{
    public static IServiceCollection AddMetricsPipeline<T>(this IServiceCollection services, TimeSpan? interval = null)
    {
        services.AddSingleton<ISummarisationService, AverageSummarisationService>();
        services.AddSingleton<IMetricsGatherer, InMemoryGatherer>(_ => new InMemoryGatherer(Array.Empty<decimal>()));
        services.AddSingleton<PipelineOrchestrator<T>>();
        services.AddHostedService(sp => new PipelineWorker<T>(
            sp.GetRequiredService<PipelineOrchestrator<T>>(),
            sp.GetRequiredService<ILogger<PipelineWorker<T>>>(),
            interval ?? TimeSpan.FromMinutes(5)));
        return services;
    }
}
