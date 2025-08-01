using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Validation.Infrastructure.Pipeline;

public class PipelineWorkerOptions
{
    public int IntervalMs { get; set; } = 60000;
    public bool Enabled { get; set; } = true;
}

public class PipelineWorker : BackgroundService
{
    private readonly PipelineOrchestrator _pipeline;
    private readonly ILogger<PipelineWorker> _logger;
    private readonly PipelineWorkerOptions _options;

    public PipelineWorker(PipelineOrchestrator pipeline, ILogger<PipelineWorker> logger, IOptions<PipelineWorkerOptions> options)
    {
        _pipeline = pipeline;
        _logger = logger;
        _options = options.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_options.Enabled)
            return;

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await _pipeline.ExecuteAsync<object>(stoppingToken);
                await Task.Delay(_options.IntervalMs, stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Pipeline worker error");
                await Task.Delay(5000, stoppingToken);
            }
        }
    }
}
