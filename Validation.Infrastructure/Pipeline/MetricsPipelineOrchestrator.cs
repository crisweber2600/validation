using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Validation.Infrastructure.Metrics;

namespace Validation.Infrastructure.Pipeline;

/// <summary>
/// Pipeline orchestrator for metrics processing workflows
/// </summary>
public class MetricsPipelineOrchestrator : IPipelineOrchestrator<MetricsInput>
{
    private readonly IMetricsCollector _metricsCollector;
    private readonly ILogger<MetricsPipelineOrchestrator> _logger;

    public MetricsPipelineOrchestrator(
        IMetricsCollector metricsCollector,
        ILogger<MetricsPipelineOrchestrator> logger)
    {
        _metricsCollector = metricsCollector;
        _logger = logger;
    }

    public async Task<PipelineResult> ExecuteAsync(MetricsInput input)
    {
        var stopwatch = Stopwatch.StartNew();
        
        try
        {
            _logger.LogInformation("Starting metrics pipeline for {EntityType}", input.EntityType);

            // Record the validation metrics
            _metricsCollector.RecordValidationDuration(input.EntityType, input.DurationMs);
            _metricsCollector.RecordValidationResult(input.EntityType, input.Success);

            if (input.RetryAttempt > 0)
            {
                _metricsCollector.RecordRetryAttempt($"{input.EntityType}_validation", input.RetryAttempt);
            }

            // Get current metrics summary
            var summary = await _metricsCollector.GetMetricsSummaryAsync(TimeSpan.FromMinutes(5));

            stopwatch.Stop();
            _logger.LogInformation(
                "Metrics pipeline completed for {EntityType} in {Duration}ms",
                input.EntityType,
                stopwatch.ElapsedMilliseconds);

            return PipelineResult.Successful(summary, stopwatch.Elapsed);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "Metrics pipeline failed for {EntityType}", input.EntityType);
            return PipelineResult.Failed(ex.Message, stopwatch.Elapsed);
        }
    }

    public async Task<PipelineResult> ExecuteWithWorkerAsync(MetricsInput input, WorkerConfig config)
    {
        // For metrics processing, worker config mainly affects retry behavior
        var retryCount = 0;
        PipelineResult result;

        do
        {
            result = await ExecuteAsync(input);
            if (result.Success)
                break;

            retryCount++;
            if (retryCount < config.MaxRetries)
            {
                await Task.Delay(TimeSpan.FromSeconds(Math.Pow(2, retryCount))); // Exponential backoff
            }
        } while (retryCount < config.MaxRetries);

        return result;
    }
}

/// <summary>
/// Input data for metrics pipeline
/// </summary>
public class MetricsInput
{
    public string EntityType { get; set; } = string.Empty;
    public double DurationMs { get; set; }
    public bool Success { get; set; }
    public int RetryAttempt { get; set; }
    public Dictionary<string, object> AdditionalData { get; set; } = new();
}