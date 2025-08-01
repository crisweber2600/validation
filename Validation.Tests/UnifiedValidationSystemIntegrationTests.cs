using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Validation.Infrastructure.Pipeline;
using Validation.Infrastructure.Messaging;
using Validation.Infrastructure.Metrics;
using Validation.Domain.Events;
using Xunit;

namespace Validation.Tests;

public class UnifiedValidationSystemIntegrationTests
{
    [Fact]
    public void ValidationEventHub_CanBeRegisteredAndResolved()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<IValidationEventHub, ValidationEventHub>();
        services.AddSingleton<ILogger<ValidationEventHub>>(_ => NullLogger<ValidationEventHub>.Instance);
        
        // Act
        var provider = services.BuildServiceProvider();
        var eventHub = provider.GetService<IValidationEventHub>();
        
        // Assert
        Assert.NotNull(eventHub);
    }

    [Fact]
    public async Task ValidationEventHub_CanPublishAndRetrieveEvents()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<IValidationEventHub, ValidationEventHub>();
        services.AddSingleton<ILogger<ValidationEventHub>>(_ => NullLogger<ValidationEventHub>.Instance);
        
        var provider = services.BuildServiceProvider();
        var eventHub = provider.GetRequiredService<IValidationEventHub>();
        
        var testEvent = new SaveValidationCompleted(
            Guid.NewGuid(), 
            "TestEntity", 
            true, 
            "test payload");
        
        // Act
        await eventHub.PublishAsync(testEvent);
        var events = await eventHub.GetEventsAsync("TestEntity");
        
        // Assert
        Assert.Single(events);
        Assert.Equal(testEvent.EntityId, events.First().EntityId);
    }

    [Fact]
    public void PipelineOrchestrators_CanBeRegistered()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<ILogger<MetricsPipelineOrchestrator>>(_ => NullLogger<MetricsPipelineOrchestrator>.Instance);
        services.AddSingleton<ILogger<SummarisationPipelineOrchestrator>>(_ => NullLogger<SummarisationPipelineOrchestrator>.Instance);
        services.AddScoped<IPipelineOrchestrator<MetricsInput>, MetricsPipelineOrchestrator>();
        services.AddScoped<IPipelineOrchestrator<SummarisationInput>, SummarisationPipelineOrchestrator>();
        
        // Mock metrics collector for this test
        var mockMetricsCollector = new MockMetricsCollector();
        services.AddSingleton<IMetricsCollector>(mockMetricsCollector);
        
        // Act
        var provider = services.BuildServiceProvider();
        var metricsOrchestrator = provider.GetService<IPipelineOrchestrator<MetricsInput>>();
        var summarisationOrchestrator = provider.GetService<IPipelineOrchestrator<SummarisationInput>>();
        
        // Assert
        Assert.NotNull(metricsOrchestrator);
        Assert.NotNull(summarisationOrchestrator);
    }

    [Fact]
    public async Task MetricsPipelineOrchestrator_CanExecuteSuccessfully()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<ILogger<MetricsPipelineOrchestrator>>(_ => NullLogger<MetricsPipelineOrchestrator>.Instance);
        var mockMetricsCollector = new MockMetricsCollector();
        services.AddSingleton<IMetricsCollector>(mockMetricsCollector);
        
        var provider = services.BuildServiceProvider();
        var orchestrator = new MetricsPipelineOrchestrator(mockMetricsCollector, NullLogger<MetricsPipelineOrchestrator>.Instance);
        
        var input = new MetricsInput
        {
            EntityType = "TestEntity",
            DurationMs = 100,
            Success = true,
            RetryAttempt = 0
        };
        
        // Act
        var result = await orchestrator.ExecuteAsync(input);
        
        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Data);
    }
}

// Simple mock for testing
public class MockMetricsCollector : IMetricsCollector
{
    public void RecordValidationDuration(string entityType, double durationMs) { }
    public void RecordValidationResult(string entityType, bool success) { }
    public void RecordCircuitBreakerState(string operation, bool isOpen) { }
    public void RecordRetryAttempt(string operation, int attemptNumber) { }
    
    public Task<MetricsSummary> GetMetricsSummaryAsync(TimeSpan? period = null)
    {
        return Task.FromResult(new MetricsSummary
        {
            Period = period ?? TimeSpan.FromMinutes(5),
            TotalValidations = 1,
            SuccessfulValidations = 1,
            FailedValidations = 0,
            AverageValidationDuration = 100
        });
    }
}