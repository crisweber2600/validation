using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging.Abstractions;
using Validation.Infrastructure.Metrics;
using Validation.Infrastructure.Pipeline;
using Xunit;

namespace Validation.Tests;

public class PipelineOrchestratorTests
{
    private class TestGather : IGatherService
    {
        public IEnumerable<int> Data { get; set; } = new List<int>();
        public Task<IEnumerable<T>> GatherAsync<T>(CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IEnumerable<T>>(Data.Cast<T>().ToList());
        }
    }

    private class SumService : ISummarisationService
    {
        public Task<T> SummariseAsync<T>(IEnumerable<T> items, CancellationToken cancellationToken = default)
        {
            var sum = items.Cast<int>().Sum();
            return Task.FromResult((T)(object)sum);
        }
    }

    private class TestValidator : IValidationService
    {
        public bool Result { get; set; }
        public Task<bool> ValidateAsync<T>(T summary, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(Result);
        }
    }

    private class TestCommit : ICommitService
    {
        public int CommitCount { get; private set; }
        public Task CommitAsync<T>(T summary, CancellationToken cancellationToken = default)
        {
            CommitCount++;
            return Task.CompletedTask;
        }
    }

    [Fact]
    public async Task ExecuteAsync_Success_Commits()
    {
        var gather = new TestGather { Data = new[] { 1, 2, 3 } };
        var summarise = new SumService();
        var validator = new TestValidator { Result = true };
        var commit = new TestCommit();
        var metrics = new MetricsCollector(NullLogger<MetricsCollector>.Instance);
        var orchestrator = new PipelineOrchestrator(gather, summarise, validator, commit, metrics, NullLogger<PipelineOrchestrator>.Instance);

        await orchestrator.ExecuteAsync<int>(CancellationToken.None);

        Assert.Equal(1, commit.CommitCount);
        var summary = await metrics.GetMetricsSummaryAsync();
        Assert.Equal(1, summary.SuccessfulValidations);
    }

    [Fact]
    public async Task ExecuteAsync_ValidationFails_Discards()
    {
        var gather = new TestGather { Data = new[] { 1, 2 } };
        var summarise = new SumService();
        var validator = new TestValidator { Result = false };
        var commit = new TestCommit();
        var metrics = new MetricsCollector(NullLogger<MetricsCollector>.Instance);
        var orchestrator = new PipelineOrchestrator(gather, summarise, validator, commit, metrics, NullLogger<PipelineOrchestrator>.Instance);

        var discarded = false;
        orchestrator.Discarded += _ => discarded = true;

        await orchestrator.ExecuteAsync<int>(CancellationToken.None);

        Assert.True(discarded);
        Assert.Equal(0, commit.CommitCount);
        var summary = await metrics.GetMetricsSummaryAsync();
        Assert.Equal(1, summary.FailedValidations);
    }
}
