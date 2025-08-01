using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace Validation.Infrastructure.Pipeline;

public class PipelineWorker : BackgroundService
{
    private readonly PipelineOrchestrator _orchestrator;
    private readonly TimeSpan _delay;

    public PipelineWorker(PipelineOrchestrator orchestrator)
    {
        _orchestrator = orchestrator;
        _delay = TimeSpan.FromSeconds(30);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await _orchestrator.RunPipelineAsync(stoppingToken);
            await Task.Delay(_delay, stoppingToken);
        }
    }
}
