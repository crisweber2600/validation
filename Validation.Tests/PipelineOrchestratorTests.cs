using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Validation.Infrastructure.Metrics;
using Validation.Domain.Validation;
using Xunit;

namespace Validation.Tests;

public class PipelineOrchestratorTests
{
    private class TestGatherer : ISummaryGatherer
    {
        private Func<IEnumerable<decimal>> _get;
        public TestGatherer(Func<IEnumerable<decimal>> get) => _get = get;
        public void Set(Func<IEnumerable<decimal>> f) => _get = f;
        public Task<IEnumerable<decimal>> GatherAsync(CancellationToken ct = default) => Task.FromResult(_get());
    }

    private class SumService : ISummarisationService
    {
        public Task<decimal> SummariseAsync(IEnumerable<decimal> values, CancellationToken ct = default)
        {
            decimal sum = 0;
            foreach (var v in values) sum += v;
            return Task.FromResult(sum);
        }
    }

    private class InMemoryCommitter : ISummaryCommitter
    {
        public readonly List<decimal> Committed = new();
        public Task CommitAsync(decimal summary, CancellationToken ct = default)
        {
            Committed.Add(summary);
            return Task.CompletedTask;
        }
    }

    [Fact]
    public async Task Orchestrator_gathers_summarises_validates_and_commits()
    {
        var gatherers = new ISummaryGatherer[]
        {
            new TestGatherer(new[] {1m, 2m}),
            new TestGatherer(new[] {3m})
        };
        var summariser = new SumService();
        var validator = new SummarisationValidator();
        var plan = new ValidationPlan(_ => 0m, ThresholdType.RawDifference, 10m);
        var committer = new InMemoryCommitter();
        var orchestrator = new PipelineOrchestrator(gatherers, summariser, validator, plan, committer);

        await orchestrator.ExecuteAsync();

        Assert.Single(committer.Committed);
        Assert.Equal(6m, committer.Committed[0]);
    }

    [Fact]
    public async Task Orchestrator_does_not_commit_when_validation_fails()
    {
        var tg = new TestGatherer(() => new[] {5m});
        var gatherers = new ISummaryGatherer[] { tg };
        var summariser = new SumService();
        var validator = new SummarisationValidator();
        var plan = new ValidationPlan(_ => 0m, ThresholdType.RawDifference, 1m);
        var committer = new InMemoryCommitter();
        var orchestrator = new PipelineOrchestrator(gatherers, summariser, validator, plan, committer);

        await orchestrator.ExecuteAsync();
        tg.Set(() => new[] {10m});
        await orchestrator.ExecuteAsync();

        Assert.Single(committer.Committed);
    }
}
