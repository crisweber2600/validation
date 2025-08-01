using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Validation.Infrastructure.Metrics;

public class PipelineWorker<T> : BackgroundService
{
    private readonly PipelineOrchestrator<T> _orchestrator;
    private readonly ILogger<PipelineWorker<T>> _logger;
    private readonly TimeSpan _interval;

    public PipelineWorker(PipelineOrchestrator<T> orchestrator, ILogger<PipelineWorker<T>> logger, TimeSpan interval)
    {
        _orchestrator = orchestrator;
        _logger = logger;
        _interval = interval;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await _orchestrator.RunAsync(Guid.NewGuid(), stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Pipeline execution failed");
            }

            await Task.Delay(_interval, stoppingToken);
        }
    }
}
