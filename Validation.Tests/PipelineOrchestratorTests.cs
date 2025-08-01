using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging.Abstractions;
using Validation.Domain.Validation;
using Validation.Infrastructure.Metrics;
using Xunit;

namespace Validation.Tests;

public class PipelineOrchestratorTests
{
    private class DummyGatherer : IMetricsGatherer
    {
        public Task<IEnumerable<decimal>> GatherAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IEnumerable<decimal>>(new[] { 1m, 2m, 3m });
        }
    }

    private class DummySummarisationService : ISummarisationService
    {
        public Task<decimal> SummariseAsync(IEnumerable<decimal> data, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(data.Sum());
        }
    }

    private class DummyCommitter : ISummaryCommitter
    {
        public decimal? Summary { get; private set; }
        public Task CommitAsync(decimal summary, CancellationToken cancellationToken = default)
        {
            Summary = summary;
            return Task.CompletedTask;
        }
    }

    [Fact]
    public async Task Orchestrator_runs_pipeline_and_commits()
    {
        var gatherer = new DummyGatherer();
        var summariser = new DummySummarisationService();
        var validator = new SummarisationValidator();
        var plan = new ValidationPlan(_ => 0m, ThresholdType.RawDifference, 100m);
        var committer = new DummyCommitter();

        var orchestrator = new PipelineOrchestrator(gatherer, summariser, validator, plan, committer);

        await orchestrator.RunAsync();

        Assert.Equal(6m, committer.Summary);
    }

    [Fact]
    public async Task PipelineWorker_executes_orchestrator()
    {
        var gatherer = new DummyGatherer();
        var summariser = new DummySummarisationService();
        var validator = new SummarisationValidator();
        var plan = new ValidationPlan(_ => 0m, ThresholdType.RawDifference, 100m);
        var committer = new DummyCommitter();
        var orchestrator = new PipelineOrchestrator(gatherer, summariser, validator, plan, committer);
        var options = new PipelineWorkerOptions { IntervalMs = 10 };
        var worker = new PipelineWorker(orchestrator, options, NullLogger<PipelineWorker>.Instance);

        using var cts = new CancellationTokenSource(30);
        await worker.StartAsync(cts.Token);
        await Task.Delay(15);
        await worker.StopAsync(CancellationToken.None);

        Assert.NotNull(committer.Summary);
    }
}
