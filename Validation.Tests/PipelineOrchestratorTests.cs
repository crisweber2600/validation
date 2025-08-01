using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging.Abstractions;
using Validation.Infrastructure.Metrics;
using Validation.Infrastructure.Pipeline;
using Xunit;

namespace Validation.Tests;

public class PipelineOrchestratorTests
{
    private class StubGather : IGatherService
    {
        public Task<IEnumerable<T>> GatherAsync<T>(CancellationToken ct = default)
        {
            IEnumerable<T> data = (IEnumerable<T>)(new List<int> { 1, 2, 3 }).Cast<T>();
            return Task.FromResult(data);
        }
    }

    private class StubSummarise : ISummarisationService
    {
        public Task<decimal> SummariseAsync<T>(IEnumerable<T> items, CancellationToken ct = default)
        {
            var sum = items.Cast<int>().Sum();
            return Task.FromResult((decimal)sum);
        }
    }

    private class StubValidation : IValidationService
    {
        public bool ShouldPass { get; set; } = true;
        public Task<bool> ValidateAsync<T>(decimal summary, CancellationToken ct = default)
            => Task.FromResult(ShouldPass);
    }

    private class StubCommit : ICommitService
    {
        public decimal? Committed { get; private set; }
        public Task CommitAsync<T>(decimal summary, CancellationToken ct = default)
        {
            Committed = summary;
            return Task.CompletedTask;
        }
    }

    [Fact]
    public async Task ExecuteAsync_WhenValid_CommitsAndRecordsMetrics()
    {
        var gather = new StubGather();
        var summarise = new StubSummarise();
        var validate = new StubValidation();
        var commit = new StubCommit();
        var metrics = new MetricsCollector(NullLogger<MetricsCollector>.Instance);
        var orchestrator = new PipelineOrchestrator(gather, summarise, validate, commit, metrics, NullLogger<PipelineOrchestrator>.Instance);

        await orchestrator.ExecuteAsync<int>();

        Assert.Equal(6m, commit.Committed);
        var summary = await metrics.GetMetricsSummaryAsync();
        Assert.Equal(1, summary.TotalValidations);
        Assert.Equal(1, summary.SuccessfulValidations);
    }

    [Fact]
    public async Task ExecuteAsync_WhenInvalid_DoesNotCommitAndRecordsFailure()
    {
        var gather = new StubGather();
        var summarise = new StubSummarise();
        var validate = new StubValidation { ShouldPass = false };
        var commit = new StubCommit();
        var metrics = new MetricsCollector(NullLogger<MetricsCollector>.Instance);
        var orchestrator = new PipelineOrchestrator(gather, summarise, validate, commit, metrics, NullLogger<PipelineOrchestrator>.Instance);

        await orchestrator.ExecuteAsync<int>();

        Assert.Null(commit.Committed);
        var summary = await metrics.GetMetricsSummaryAsync();
        Assert.Equal(1, summary.TotalValidations);
        Assert.Equal(1, summary.FailedValidations);
    }
}
