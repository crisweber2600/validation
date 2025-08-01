using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Validation.Infrastructure.Metrics;

public interface IMetricsCollector
{
    void RecordValidationDuration(string entityType, double durationMs);
    void RecordValidationResult(string entityType, bool success);
    void RecordCircuitBreakerState(string operation, bool isOpen);
    void RecordRetryAttempt(string operation, int attemptNumber);
    Task<MetricsSummary> GetMetricsSummaryAsync(TimeSpan? period = null);
}

public class MetricsCollector : IMetricsCollector
{
    private readonly ConcurrentQueue<MetricEvent> _events = new();
    private readonly ILogger<MetricsCollector> _logger;

    public MetricsCollector(ILogger<MetricsCollector> logger)
    {
        _logger = logger;
    }

    public void RecordValidationDuration(string entityType, double durationMs)
    {
        _events.Enqueue(new MetricEvent
        {
            Type = MetricType.ValidationDuration,
            EntityType = entityType,
            Value = durationMs,
            Timestamp = DateTime.UtcNow
        });
    }

    public void RecordValidationResult(string entityType, bool success)
    {
        _events.Enqueue(new MetricEvent
        {
            Type = MetricType.ValidationResult,
            EntityType = entityType,
            Value = success ? 1 : 0,
            Timestamp = DateTime.UtcNow
        });
    }

    public void RecordCircuitBreakerState(string operation, bool isOpen)
    {
        _events.Enqueue(new MetricEvent
        {
            Type = MetricType.CircuitBreakerState,
            Operation = operation,
            Value = isOpen ? 1 : 0,
            Timestamp = DateTime.UtcNow
        });
    }

    public void RecordRetryAttempt(string operation, int attemptNumber)
    {
        _events.Enqueue(new MetricEvent
        {
            Type = MetricType.RetryAttempt,
            Operation = operation,
            Value = attemptNumber,
            Timestamp = DateTime.UtcNow
        });
    }

    public Task<MetricsSummary> GetMetricsSummaryAsync(TimeSpan? period = null)
    {
        var cutoff = period.HasValue ? DateTime.UtcNow - period.Value : DateTime.MinValue;
        var relevantEvents = _events.Where(e => e.Timestamp >= cutoff).ToList();

        var summary = new MetricsSummary
        {
            Period = period ?? TimeSpan.FromDays(1),
            TotalValidations = relevantEvents.Count(e => e.Type == MetricType.ValidationResult),
            SuccessfulValidations = relevantEvents.Count(e => e.Type == MetricType.ValidationResult && e.Value > 0),
            FailedValidations = relevantEvents.Count(e => e.Type == MetricType.ValidationResult && e.Value == 0),
            AverageValidationDuration = relevantEvents
                .Where(e => e.Type == MetricType.ValidationDuration)
                .Select(e => e.Value)
                .DefaultIfEmpty(0)
                .Average(),
            TotalRetries = relevantEvents.Count(e => e.Type == MetricType.RetryAttempt),
            CircuitBreakerOpenCount = relevantEvents.Count(e => e.Type == MetricType.CircuitBreakerState && e.Value > 0),
            EntityTypeBreakdown = relevantEvents
                .Where(e => !string.IsNullOrEmpty(e.EntityType))
                .GroupBy(e => e.EntityType!)
                .ToDictionary(g => g.Key, g => g.Count())
        };

        return Task.FromResult(summary);
    }
}

public class MetricsOrchestrator : BackgroundService
{
    private readonly IMetricsCollector _metricsCollector;
    private readonly ILogger<MetricsOrchestrator> _logger;
    private readonly MetricsOrchestratorOptions _options;

    public MetricsOrchestrator(
        IMetricsCollector metricsCollector,
        ILogger<MetricsOrchestrator> logger,
        IOptions<MetricsOrchestratorOptions> options)
    {
        _metricsCollector = metricsCollector;
        _logger = logger;
        _options = options.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessMetricsAsync(stoppingToken);
                await Task.Delay(_options.ProcessingIntervalMs, stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing metrics");
                await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
            }
        }
    }

    private async Task ProcessMetricsAsync(CancellationToken cancellationToken)
    {
        var summary = await _metricsCollector.GetMetricsSummaryAsync(TimeSpan.FromMinutes(5));

        _logger.LogInformation(
            "Metrics Summary: {TotalValidations} validations, {SuccessRate:P2} success rate, {AvgDuration:F2}ms avg duration",
            summary.TotalValidations,
            summary.SuccessRate,
            summary.AverageValidationDuration);

        // Here you could send metrics to external systems like Prometheus, Application Insights, etc.
        if (_options.LogDetailedMetrics)
        {
            _logger.LogDebug("Detailed metrics: {@MetricsSummary}", summary);
        }
    }
}

public class MetricEvent
{
    public MetricType Type { get; set; }
    public string? EntityType { get; set; }
    public string? Operation { get; set; }
    public double Value { get; set; }
    public DateTime Timestamp { get; set; }
}

public enum MetricType
{
    ValidationDuration,
    ValidationResult,
    CircuitBreakerState,
    RetryAttempt
}

public class MetricsSummary
{
    public TimeSpan Period { get; set; }
    public int TotalValidations { get; set; }
    public int SuccessfulValidations { get; set; }
    public int FailedValidations { get; set; }
    public double AverageValidationDuration { get; set; }
    public int TotalRetries { get; set; }
    public int CircuitBreakerOpenCount { get; set; }
    public Dictionary<string, int> EntityTypeBreakdown { get; set; } = new();

    public double SuccessRate => TotalValidations > 0 ? (double)SuccessfulValidations / TotalValidations : 0;
}

public class MetricsOrchestratorOptions
{
    public int ProcessingIntervalMs { get; set; } = 60000; // 1 minute
    public bool LogDetailedMetrics { get; set; } = false;
}