using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.DependencyInjection;

namespace Validation.Infrastructure.Pipeline;

public enum PipelineWorkerMode
{
    Periodic,
    OnDemand
}

public class PipelineWorkerOptions
{
    public PipelineWorkerMode Mode { get; set; } = PipelineWorkerMode.Periodic;
    public TimeSpan Interval { get; set; } = TimeSpan.FromMinutes(1);
}

public class PipelineWorker : BackgroundService
{
    private readonly PipelineOrchestrator _orchestrator;
    private readonly ILogger<PipelineWorker> _logger;
    private readonly PipelineWorkerOptions _options;

    public PipelineWorker(PipelineOrchestrator orchestrator, ILogger<PipelineWorker> logger, IOptions<PipelineWorkerOptions> options)
    {
        _orchestrator = orchestrator;
        _logger = logger;
        _options = options.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (_options.Mode == PipelineWorkerMode.OnDemand)
        {
            await _orchestrator.ExecuteAsync<object>(stoppingToken);
            return;
        }

        while (!stoppingToken.IsCancellationRequested)
        {
            await _orchestrator.ExecuteAsync<object>(stoppingToken);
            await Task.Delay(_options.Interval, stoppingToken);
        }
    }
}

public static class MetricsPipelineExtensions
{
    public static IServiceCollection AddMetricsPipeline(this IServiceCollection services, Action<PipelineWorkerOptions>? configure = null)
    {
        services.AddSingleton<PipelineOrchestrator>();
        services.AddSingleton<PipelineWorkerOptions>(sp =>
        {
            var opts = new PipelineWorkerOptions();
            configure?.Invoke(opts);
            return opts;
        });
        services.AddHostedService<PipelineWorker>();
        return services;
    }
}
