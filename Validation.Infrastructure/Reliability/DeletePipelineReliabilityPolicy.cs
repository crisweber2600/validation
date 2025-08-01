using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Validation.Infrastructure.Reliability;

public class DeletePipelineReliabilityPolicy
{
    private readonly ILogger<DeletePipelineReliabilityPolicy> _logger;
    private readonly DeleteReliabilityOptions _options;
    private int _consecutiveFailures = 0;
    private DateTime _lastFailureTime = DateTime.MinValue;

    public DeletePipelineReliabilityPolicy(
        ILogger<DeletePipelineReliabilityPolicy> logger,
        DeleteReliabilityOptions options)
    {
        _logger = logger;
        _options = options;
    }

    public async Task<T> ExecuteAsync<T>(
        Func<CancellationToken, Task<T>> operation,
        CancellationToken cancellationToken = default)
    {
        if (IsCircuitOpen())
        {
            _logger.LogWarning("Delete pipeline circuit breaker is open. Failing fast.");
            throw new DeletePipelineCircuitOpenException("Circuit breaker is open");
        }

        var attempts = 0;
        Exception? lastException = null;

        while (attempts < _options.MaxRetryAttempts)
        {
            try
            {
                _logger.LogDebug("Executing delete pipeline operation. Attempt {Attempt}", attempts + 1);

                var result = await operation(cancellationToken);

                // Reset failure count on success
                Interlocked.Exchange(ref _consecutiveFailures, 0);
                _logger.LogDebug("Delete pipeline operation completed successfully");

                return result;
            }
            catch (Exception ex)
            {
                lastException = ex;
                attempts++;

                if (!ShouldRetry(ex))
                {
                    // Non-retryable exception - rethrow immediately
                    _logger.LogError(ex, "Delete pipeline operation failed with non-retryable exception");
                    throw;
                }

                if (attempts < _options.MaxRetryAttempts)
                {
                    _logger.LogWarning(ex,
                        "Delete pipeline operation failed. Attempt {Attempt} of {MaxAttempts}. Retrying in {DelayMs}ms",
                        attempts, _options.MaxRetryAttempts, _options.RetryDelayMs);

                    await Task.Delay(_options.RetryDelayMs, cancellationToken);
                    continue;
                }
                else
                {
                    // Retries exhausted - record failure and break to throw wrapped exception
                    Interlocked.Increment(ref _consecutiveFailures);
                    _lastFailureTime = DateTime.UtcNow;
                    break;
                }
            }
        }

        _logger.LogError(lastException, "Delete pipeline operation failed after {Attempts} attempts", attempts);
        
        // Always wrap retryable exceptions that exhausted retries in DeletePipelineReliabilityException
        throw new DeletePipelineReliabilityException(
            $"Delete pipeline operation failed after {attempts} attempts", lastException);
    }

    public async Task ExecuteAsync(
        Func<CancellationToken, Task> operation,
        CancellationToken cancellationToken = default)
    {
        await ExecuteAsync<object?>(async ct =>
        {
            await operation(ct);
            return null;
        }, cancellationToken);
    }

    private static bool ShouldRetry(Exception exception)
    {
        // Don't retry on certain exception types
        return exception is not ArgumentException and not ArgumentNullException;
    }

    private bool IsCircuitOpen()
    {
        if (_consecutiveFailures < _options.CircuitBreakerThreshold)
            return false;

        var timeSinceLastFailure = DateTime.UtcNow - _lastFailureTime;
        return timeSinceLastFailure < TimeSpan.FromMilliseconds(_options.CircuitBreakerTimeoutMs);
    }
}

public class DeleteReliabilityOptions
{
    public int MaxRetryAttempts { get; set; } = 3;
    public int RetryDelayMs { get; set; } = 1000;
    public int CircuitBreakerThreshold { get; set; } = 5;
    public int CircuitBreakerTimeoutMs { get; set; } = 30000;
}

public class DeletePipelineReliabilityException : Exception
{
    public DeletePipelineReliabilityException(string message) : base(message) { }
    public DeletePipelineReliabilityException(string message, Exception? innerException) : base(message, innerException) { }
}

public class DeletePipelineCircuitOpenException : Exception
{
    public DeletePipelineCircuitOpenException(string message) : base(message) { }
}