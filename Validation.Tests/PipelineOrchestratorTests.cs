using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging.Abstractions;
using Validation.Domain.Validation;
using Validation.Infrastructure.Metrics;
using Xunit;

namespace Validation.Tests;

public class PipelineOrchestratorTests
{
    private class TestMetricsCollector : IMetricsCollector
    {
        public List<double> Durations { get; } = new();

        public void RecordValidationDuration(string entityType, double durationMs) => Durations.Add(durationMs);
        public void RecordValidationResult(string entityType, bool success) { }
        public void RecordCircuitBreakerState(string operation, bool isOpen) { }
        public void RecordRetryAttempt(string operation, int attemptNumber) { }
        public Task<MetricsSummary> GetMetricsSummaryAsync(System.TimeSpan? period = null) => Task.FromResult(new MetricsSummary());
    }

    private class SumSummarisationService : ISummarisationService
    {
        public Task<decimal> SummariseAsync(IEnumerable<decimal> values, System.Threading.CancellationToken ct = default) => Task.FromResult(values.Sum());
    }

    [Fact]
    public async Task RunAsync_Commits_when_validation_passes()
    {
        var collector = new TestMetricsCollector();
        var gatherer = new InMemoryGatherer(new decimal[] { 1m, 2m, 3m });
        var summariser = new SumSummarisationService();
        var options = new MetricsPipelineOptions { ValidationPlan = new ValidationPlan(new[] { new AlwaysValidRule() }) };
        var orchestrator = new PipelineOrchestrator(new[] { gatherer }, summariser, new SummarisationValidator(), collector, options, NullLogger<PipelineOrchestrator>.Instance);

        await orchestrator.RunAsync();

        Assert.Single(collector.Durations);
        Assert.Equal(6, collector.Durations[0]);
    }

    private class AlwaysInvalidRule : IValidationRule
    {
        public bool Validate(decimal previousValue, decimal newValue) => false;
    }

    [Fact]
    public async Task RunAsync_Does_not_commit_when_validation_fails()
    {
        var collector = new TestMetricsCollector();
        var gatherer = new InMemoryGatherer(new decimal[] { 1m });
        var summariser = new SumSummarisationService();
        var options = new MetricsPipelineOptions { ValidationPlan = new ValidationPlan(new[] { new AlwaysInvalidRule() }) };
        var orchestrator = new PipelineOrchestrator(new[] { gatherer }, summariser, new SummarisationValidator(), collector, options, NullLogger<PipelineOrchestrator>.Instance);

        await orchestrator.RunAsync();

        Assert.Empty(collector.Durations);
    }
}
