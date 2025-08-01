using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging.Abstractions;
using Validation.Infrastructure.Metrics;
using Xunit;

namespace Validation.Tests;

public class MetricsCollectorTests
{
    private readonly MetricsCollector _metricsCollector;

    public MetricsCollectorTests()
    {
        _metricsCollector = new MetricsCollector(NullLogger<MetricsCollector>.Instance);
    }

    [Fact]
    public async Task RecordValidationDuration_AddsMetric()
    {
        // Act
        _metricsCollector.RecordValidationDuration("TestEntity", 150.5);

        // Assert - No direct way to verify without exposing internals, but we can verify via summary
        var summary = await _metricsCollector.GetMetricsSummaryAsync();
        Assert.True(summary.AverageValidationDuration > 0);
    }

    [Fact]
    public async Task RecordValidationResult_Success_IncreasesSuccessCount()
    {
        // Act
        _metricsCollector.RecordValidationResult("TestEntity", true);

        // Assert
        var summary = await _metricsCollector.GetMetricsSummaryAsync();
        Assert.Equal(1, summary.SuccessfulValidations);
        Assert.Equal(1, summary.TotalValidations);
        Assert.Equal(0, summary.FailedValidations);
    }

    [Fact]
    public async Task RecordValidationResult_Failure_IncreasesFailureCount()
    {
        // Act
        _metricsCollector.RecordValidationResult("TestEntity", false);

        // Assert
        var summary = await _metricsCollector.GetMetricsSummaryAsync();
        Assert.Equal(0, summary.SuccessfulValidations);
        Assert.Equal(1, summary.TotalValidations);
        Assert.Equal(1, summary.FailedValidations);
    }

    [Fact]
    public async Task RecordCircuitBreakerState_Open_IncreasesOpenCount()
    {
        // Act
        _metricsCollector.RecordCircuitBreakerState("delete", true);

        // Assert
        var summary = await _metricsCollector.GetMetricsSummaryAsync();
        Assert.Equal(1, summary.CircuitBreakerOpenCount);
    }

    [Fact]
    public async Task RecordRetryAttempt_IncreasesRetryCount()
    {
        // Act
        _metricsCollector.RecordRetryAttempt("save", 2);

        // Assert
        var summary = await _metricsCollector.GetMetricsSummaryAsync();
        Assert.Equal(1, summary.TotalRetries);
    }

    [Fact]
    public async Task GetMetricsSummaryAsync_WithPeriod_FiltersCorrectly()
    {
        // Arrange
        _metricsCollector.RecordValidationResult("TestEntity", true);
        await Task.Delay(100); // Ensure time difference

        // Act
        var recentSummary = await _metricsCollector.GetMetricsSummaryAsync(TimeSpan.FromMilliseconds(50));
        var allTimeSummary = await _metricsCollector.GetMetricsSummaryAsync(TimeSpan.FromDays(1));

        // Assert
        Assert.Equal(0, recentSummary.TotalValidations); // Should be filtered out
        Assert.Equal(1, allTimeSummary.TotalValidations); // Should be included
    }

    [Fact]
    public async Task GetMetricsSummaryAsync_MultipleEntityTypes_BreaksDownCorrectly()
    {
        // Arrange
        _metricsCollector.RecordValidationResult("EntityA", true);
        _metricsCollector.RecordValidationResult("EntityA", true);
        _metricsCollector.RecordValidationResult("EntityB", false);

        // Act
        var summary = await _metricsCollector.GetMetricsSummaryAsync();

        // Assert
        Assert.True(summary.EntityTypeBreakdown.ContainsKey("EntityA"));
        Assert.True(summary.EntityTypeBreakdown.ContainsKey("EntityB"));
        Assert.Equal(2, summary.EntityTypeBreakdown["EntityA"]);
        Assert.Equal(1, summary.EntityTypeBreakdown["EntityB"]);
    }

    [Fact]
    public async Task GetMetricsSummaryAsync_CalculatesSuccessRateCorrectly()
    {
        // Arrange
        _metricsCollector.RecordValidationResult("TestEntity", true);
        _metricsCollector.RecordValidationResult("TestEntity", true);
        _metricsCollector.RecordValidationResult("TestEntity", false);

        // Act
        var summary = await _metricsCollector.GetMetricsSummaryAsync();

        // Assert
        Assert.Equal(3, summary.TotalValidations);
        Assert.Equal(2, summary.SuccessfulValidations);
        Assert.Equal(1, summary.FailedValidations);
        Assert.Equal(2.0 / 3.0, summary.SuccessRate, 2);
    }

    [Fact]
    public async Task GetMetricsSummaryAsync_NoMetrics_ReturnsZeroes()
    {
        // Act
        var summary = await _metricsCollector.GetMetricsSummaryAsync();

        // Assert
        Assert.Equal(0, summary.TotalValidations);
        Assert.Equal(0, summary.SuccessfulValidations);
        Assert.Equal(0, summary.FailedValidations);
        Assert.Equal(0, summary.AverageValidationDuration);
        Assert.Equal(0, summary.TotalRetries);
        Assert.Equal(0, summary.CircuitBreakerOpenCount);
        Assert.Equal(0, summary.SuccessRate);
        Assert.Empty(summary.EntityTypeBreakdown);
    }

    [Fact]
    public async Task GetMetricsSummaryAsync_MixedDurations_CalculatesAverageCorrectly()
    {
        // Arrange
        _metricsCollector.RecordValidationDuration("TestEntity", 100);
        _metricsCollector.RecordValidationDuration("TestEntity", 200);
        _metricsCollector.RecordValidationDuration("TestEntity", 300);

        // Act
        var summary = await _metricsCollector.GetMetricsSummaryAsync();

        // Assert
        Assert.Equal(200, summary.AverageValidationDuration);
    }
}