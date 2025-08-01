using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Validation.Infrastructure.Metrics.Pipeline;

public class PipelineWorker : BackgroundService
{
    private readonly PipelineOrchestrator _orchestrator;
    private readonly MetricsPipelineOptions _options;

    public PipelineWorker(PipelineOrchestrator orchestrator, IOptions<MetricsPipelineOptions> options)
    {
        _orchestrator = orchestrator;
        _options = options.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await _orchestrator.RunAsync(stoppingToken);
            await Task.Delay(_options.RunInterval, stoppingToken);
        }
    }
}
