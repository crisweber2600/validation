using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging.Abstractions;
using Validation.Domain.Validation;
using Validation.Infrastructure;
using Validation.Infrastructure.Metrics;
using Xunit;

namespace Validation.Tests;

public class PipelineOrchestratorTests
{
    private class TestGatherer : IMetricGatherer
    {
        private readonly IEnumerable<decimal> _values;
        public TestGatherer(IEnumerable<decimal> values) => _values = values;
        public Task<IEnumerable<decimal>> GatherAsync(CancellationToken ct = default) => Task.FromResult(_values);
    }

    private class RecordingSummarisationService : ISummarisationService
    {
        public List<decimal> Received { get; } = new();
        public decimal Result { get; set; }
        public decimal Summarise(IEnumerable<decimal> values)
        {
            Received.AddRange(values);
            return Result;
        }
    }

    private class GreaterThanRule : IValidationRule
    {
        private readonly decimal _threshold;
        public GreaterThanRule(decimal threshold) => _threshold = threshold;
        public bool Validate(decimal previous, decimal current) => current > _threshold;
    }

    [Fact]
    public async Task ExecuteAsync_gathers_summarises_validates_and_commits_success()
    {
        var gatherers = new IMetricGatherer[]
        {
            new TestGatherer(new[] {1m, 2m}),
            new TestGatherer(new[] {3m})
        };
        var summariser = new RecordingSummarisationService { Result = 6m };
        var metrics = new MetricsCollector(NullLogger<MetricsCollector>.Instance);
        var orchestrator = new PipelineOrchestrator(gatherers, summariser, new[] { new GreaterThanRule(0m) }, new SummarisationValidator(), metrics, NullLogger<PipelineOrchestrator>.Instance);

        await orchestrator.ExecuteAsync();

        Assert.Equal(new[] {1m, 2m, 3m}, summariser.Received);
        var summary = await metrics.GetMetricsSummaryAsync();
        Assert.Equal(1, summary.SuccessfulValidations);
        Assert.Equal(1, summary.TotalValidations);
    }

    [Fact]
    public async Task ExecuteAsync_records_failure_when_validation_fails()
    {
        var gatherers = new IMetricGatherer[] { new TestGatherer(new[] {1m}) };
        var summariser = new RecordingSummarisationService { Result = 1m };
        var metrics = new MetricsCollector(NullLogger<MetricsCollector>.Instance);
        var orchestrator = new PipelineOrchestrator(gatherers, summariser, new[] { new GreaterThanRule(5m) }, new SummarisationValidator(), metrics, NullLogger<PipelineOrchestrator>.Instance);

        await orchestrator.ExecuteAsync();

        var summary = await metrics.GetMetricsSummaryAsync();
        Assert.Equal(1, summary.FailedValidations);
        Assert.Equal(1, summary.TotalValidations);
    }
}
