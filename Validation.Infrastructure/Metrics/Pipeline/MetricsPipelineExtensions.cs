using System;
using Microsoft.Extensions.DependencyInjection;
using Validation.Domain.Validation;

namespace Validation.Infrastructure.Metrics;

public static class MetricsPipelineExtensions
{
    public static IServiceCollection AddMetricsPipeline(this IServiceCollection services, Action<PipelineWorkerOptions>? configure = null)
    {
        services.AddSingleton<IMetricsGatherer, InMemoryMetricsGatherer>();
        services.AddSingleton<ISummarisationService, AverageSummarisationService>();
        services.AddSingleton<ISummaryCommitter, InMemorySummaryCommitter>();
        services.AddSingleton<SummarisationValidator>();
        services.AddSingleton(new ValidationPlan(_ => 0m, ThresholdType.RawDifference, 100m));
        services.AddSingleton<PipelineOrchestrator>();
        var options = new PipelineWorkerOptions();
        configure?.Invoke(options);
        services.AddSingleton(options);
        services.AddHostedService<PipelineWorker>();
        return services;
    }
}
