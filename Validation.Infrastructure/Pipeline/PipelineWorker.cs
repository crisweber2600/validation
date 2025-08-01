using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Validation.Infrastructure.Pipeline;

public class PipelineWorkerOptions
{
    public int IntervalMs { get; set; } = 60000;
    public bool RunOnce { get; set; } = false;
    public Type DataType { get; set; } = typeof(object);
}

public class PipelineWorker : BackgroundService
{
    private readonly PipelineOrchestrator _orchestrator;
    private readonly ILogger<PipelineWorker> _logger;
    private readonly PipelineWorkerOptions _options;

    public PipelineWorker(PipelineOrchestrator orchestrator, IOptions<PipelineWorkerOptions> options, ILogger<PipelineWorker> logger)
    {
        _orchestrator = orchestrator;
        _logger = logger;
        _options = options.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        do
        {
            try
            {
                var method = typeof(PipelineOrchestrator).GetMethod("ExecuteAsync")!.MakeGenericMethod(_options.DataType);
                var task = (Task)method.Invoke(_orchestrator, new object?[] { stoppingToken })!;
                await task.ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Pipeline execution failed");
            }

            if (_options.RunOnce)
                break;

            await Task.Delay(_options.IntervalMs, stoppingToken);
        } while (!stoppingToken.IsCancellationRequested);
    }
}
