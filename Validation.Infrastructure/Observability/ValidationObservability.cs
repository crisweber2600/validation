using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Serilog.Context;

namespace Validation.Infrastructure.Observability;

public static class ValidationObservability
{
    public static readonly ActivitySource ActivitySource = new("Validation.Infrastructure", "1.0.0");

    public static class EventIds
    {
        public static readonly EventId ValidationStarted = new(1001, "ValidationStarted");
        public static readonly EventId ValidationCompleted = new(1002, "ValidationCompleted");
        public static readonly EventId ValidationFailed = new(1003, "ValidationFailed");
        public static readonly EventId DeleteValidationStarted = new(1004, "DeleteValidationStarted");
        public static readonly EventId DeleteValidationCompleted = new(1005, "DeleteValidationCompleted");
        public static readonly EventId DeleteValidationFailed = new(1006, "DeleteValidationFailed");
        public static readonly EventId CircuitBreakerOpened = new(1007, "CircuitBreakerOpened");
        public static readonly EventId CircuitBreakerClosed = new(1008, "CircuitBreakerClosed");
        public static readonly EventId RetryAttempt = new(1009, "RetryAttempt");
        public static readonly EventId MetricProcessed = new(1010, "MetricProcessed");
        public static readonly EventId AuditRecordCreated = new(1011, "AuditRecordCreated");
    }

    public static Activity? StartValidationActivity(string operationName, Guid entityId, string entityType)
    {
        var activity = ActivitySource.StartActivity($"validation.{operationName}");
        activity?.SetTag("entity.id", entityId.ToString());
        activity?.SetTag("entity.type", entityType);
        activity?.SetTag("operation.name", operationName);
        return activity;
    }

    public static IDisposable EnrichLogsWithContext(Guid entityId, string entityType, string operation)
    {
        // Create a composite disposable to handle multiple context properties
        var disposables = new List<IDisposable>
        {
            LogContext.PushProperty("EntityId", entityId),
            LogContext.PushProperty("EntityType", entityType),
            LogContext.PushProperty("Operation", operation),
            LogContext.PushProperty("CorrelationId", Guid.NewGuid())
        };

        return new CompositeDisposable(disposables);
    }

    public static void LogValidationMetrics(ILogger logger, string operation, double duration, bool success, string entityType)
    {
        using var activity = ActivitySource.StartActivity("validation.metrics");
        activity?.SetTag("operation", operation);
        activity?.SetTag("duration_ms", duration);
        activity?.SetTag("success", success);
        activity?.SetTag("entity_type", entityType);

        logger.LogInformation(EventIds.MetricProcessed,
            "Validation metric: {Operation} for {EntityType} took {Duration}ms with result {Success}",
            operation, entityType, duration, success);
    }
}

public class CompositeDisposable : IDisposable
{
    private readonly List<IDisposable> _disposables;
    private bool _disposed;

    public CompositeDisposable(IEnumerable<IDisposable> disposables)
    {
        _disposables = new List<IDisposable>(disposables);
    }

    public void Dispose()
    {
        if (_disposed) return;

        foreach (var disposable in _disposables)
        {
            try
            {
                disposable?.Dispose();
            }
            catch
            {
                // Ignore disposal errors
            }
        }

        _disposed = true;
    }
}