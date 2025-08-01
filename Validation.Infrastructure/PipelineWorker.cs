using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace Validation.Infrastructure;

public class PipelineWorker : BackgroundService
{
    private readonly PipelineOrchestrator _orchestrator;
    private readonly ILogger<PipelineWorker> _logger;
    private readonly int _intervalMs;

    public PipelineWorker(PipelineOrchestrator orchestrator, ILogger<PipelineWorker> logger, PipelineWorkerOptions options)
    {
        _orchestrator = orchestrator;
        _logger = logger;
        _intervalMs = options.IntervalMs;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await _orchestrator.ExecuteAsync(stoppingToken);
            await Task.Delay(_intervalMs, stoppingToken);
        }
    }
}

public class PipelineWorkerOptions
{
    public int IntervalMs { get; set; } = 60000;
}
