using System;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Hosting;

namespace Validation.Infrastructure;

public class PipelineWorker : BackgroundService
{
    private readonly PipelineOrchestrator _orchestrator;
    private readonly PipelineWorkerOptions _options;

    public PipelineWorker(PipelineOrchestrator orchestrator, PipelineWorkerOptions options)
    {
        _orchestrator = orchestrator;
        _options = options;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await _orchestrator.ExecuteAsync(stoppingToken);
            await Task.Delay(_options.Interval, stoppingToken);
        }
    }
}
