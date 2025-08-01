using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.DependencyInjection;

namespace Validation.Infrastructure.Pipeline;

public enum PipelineWorkerMode { Periodic, Manual }

public class PipelineWorkerOptions
{
    public PipelineWorkerMode Mode { get; set; } = PipelineWorkerMode.Periodic;
    public int IntervalMs { get; set; } = 60000;
}

public class PipelineWorker : BackgroundService
{
    private readonly IServiceProvider _provider;
    private readonly PipelineWorkerOptions _options;
    private readonly ILogger<PipelineWorker> _logger;

    public PipelineWorker(IServiceProvider provider, IOptions<PipelineWorkerOptions> options, ILogger<PipelineWorker> logger)
    {
        _provider = provider;
        _options = options.Value;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (_options.Mode != PipelineWorkerMode.Periodic)
            return;

        while (!stoppingToken.IsCancellationRequested)
        {
            using var scope = _provider.CreateScope();
            var orchestrator = scope.ServiceProvider.GetRequiredService<PipelineOrchestrator>();
            await orchestrator.ExecuteAsync(stoppingToken);
            await Task.Delay(_options.IntervalMs, stoppingToken);
        }
    }

    public async Task RunOnceAsync(CancellationToken cancellationToken = default)
    {
        using var scope = _provider.CreateScope();
        var orchestrator = scope.ServiceProvider.GetRequiredService<PipelineOrchestrator>();
        await orchestrator.ExecuteAsync(cancellationToken);
    }
}
