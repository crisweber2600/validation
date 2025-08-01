using Microsoft.Extensions.DependencyInjection;
using System.Linq;

namespace Validation.Infrastructure.Pipeline;

public static class MetricsPipelineServiceCollectionExtensions
{
    public static IServiceCollection AddMetricsPipeline(this IServiceCollection services)
    {
        services.AddSingleton<PipelineOrchestrator>();
        services.AddSingleton<PipelineWorkerOptions>();
        services.AddSingleton<IGatherService, InMemoryGatherService>();
        services.AddSingleton<ISummarisationService, SumSummarisationService>();
        services.AddSingleton<IValidationService, PassThroughValidationService>();
        services.AddSingleton<ICommitService, NoOpCommitService>();
        services.AddHostedService<PipelineWorker>();
        return services;
    }
}

// Minimal placeholder implementations
public class SumSummarisationService : ISummarisationService
{
    public Task<T> SummariseAsync<T>(IEnumerable<T> items, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(items.Last());
    }
}

public class PassThroughValidationService : IValidationService
{
    public Task<bool> ValidateAsync<T>(T summary, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(true);
    }
}

public class NoOpCommitService : ICommitService
{
    public Task CommitAsync<T>(T summary, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }
}
