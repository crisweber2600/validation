using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Validation.Infrastructure.Reliability;
using Xunit;

namespace Validation.Tests;

public class DeletePipelineReliabilityTests
{
    private readonly DeletePipelineReliabilityPolicy _policy;
    private readonly DeleteReliabilityOptions _options;

    public DeletePipelineReliabilityTests()
    {
        _options = new DeleteReliabilityOptions
        {
            MaxRetryAttempts = 3,
            RetryDelayMs = 100,
            CircuitBreakerThreshold = 2,
            CircuitBreakerTimeoutMs = 1000
        };
        _policy = new DeletePipelineReliabilityPolicy(NullLogger<DeletePipelineReliabilityPolicy>.Instance, _options);
    }

    [Fact]
    public async Task ExecuteAsync_SuccessfulOperation_ReturnsResult()
    {
        // Arrange
        const string expectedResult = "success";

        // Act
        var result = await _policy.ExecuteAsync<string>(_ => Task.FromResult(expectedResult));

        // Assert
        Assert.Equal(expectedResult, result);
    }

    [Fact]
    public async Task ExecuteAsync_TransientFailureThenSuccess_ReturnsResult()
    {
        // Arrange
        var attempts = 0;
        const string expectedResult = "success";

        // Act
        var result = await _policy.ExecuteAsync<string>(_ =>
        {
            attempts++;
            if (attempts == 1)
                throw new InvalidOperationException("Transient failure");
            return Task.FromResult(expectedResult);
        });

        // Assert
        Assert.Equal(expectedResult, result);
        Assert.Equal(2, attempts);
    }

    [Fact]
    public async Task ExecuteAsync_PermanentFailure_ThrowsAfterMaxRetries()
    {
        // Arrange
        var attempts = 0;

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _policy.ExecuteWithSyncOperationAsync<string>(_ =>
            {
                attempts++;
                throw new InvalidOperationException("Permanent failure");
            }));

        Assert.Equal(_options.MaxRetryAttempts, attempts);
        Assert.Equal("Permanent failure", exception.Message);
    }

    [Fact]
    public async Task ExecuteAsync_NonRetryableException_FailsImmediately()
    {
        // Arrange
        var attempts = 0;

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentNullException>(() =>
            _policy.ExecuteWithSyncOperationAsync<string>(_ =>
            {
                attempts++;
                throw new ArgumentNullException("paramName", "Non-retryable failure");
            }));

        Assert.Equal(1, attempts);
    }

    [Fact]
    public async Task ExecuteAsync_CircuitBreakerOpen_ThrowsCircuitOpenException()
    {
        // Arrange - cause circuit breaker to open by forcing retryable failures
        for (int i = 0; i < _options.CircuitBreakerThreshold; i++)
        {
            try
            {
                await _policy.ExecuteWithSyncOperationAsync<string>(_ => throw new InvalidOperationException("Retryable failure"));
            }
            catch (Exception)
            {
                // Ignore - failures used to open circuit
            }
        }

        // Act & Assert
        await Assert.ThrowsAsync<DeletePipelineCircuitOpenException>(async () =>
            await _policy.ExecuteAsync<string>(_ => Task.FromResult("should not execute")));
    }

    [Fact]
    public async Task ExecuteAsync_VoidOperation_CompletesSuccessfully()
    {
        // Arrange
        var executed = false;

        // Act
        await _policy.ExecuteAsync(_ =>
        {
            executed = true;
            return Task.CompletedTask;
        });

        // Assert
        Assert.True(executed);
    }
}