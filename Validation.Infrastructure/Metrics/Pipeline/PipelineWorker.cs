using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Validation.Infrastructure.Metrics;

public class PipelineWorker : BackgroundService
{
    private readonly PipelineOrchestrator _orchestrator;
    private readonly MetricsPipelineOptions _options;
    private readonly ILogger<PipelineWorker> _logger;

    public PipelineWorker(PipelineOrchestrator orchestrator, MetricsPipelineOptions options, ILogger<PipelineWorker> logger)
    {
        _orchestrator = orchestrator;
        _options = options;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await _orchestrator.RunAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error running metrics pipeline");
            }

            await Task.Delay(_options.IntervalMs, stoppingToken);
        }
    }
}
