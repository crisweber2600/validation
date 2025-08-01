using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Validation.Infrastructure.Metrics.Pipeline;
using Xunit;

namespace Validation.Tests;

public class PipelineOrchestratorTests
{
    private class TestGatherer : IMetricGatherer
    {
        private readonly IEnumerable<double> _data;
        public bool Called { get; private set; }
        public TestGatherer(IEnumerable<double> data) => _data = data;
        public Task<IEnumerable<double>> GatherAsync(CancellationToken cancellationToken = default)
        {
            Called = true;
            return Task.FromResult(_data);
        }
    }

    private class TestSummarisation : ISummarisationService
    {
        public bool Summarised { get; protected set; }
        public bool Validated { get; protected set; }
        public bool Committed { get; protected set; }
        public double Summary { get; protected set; }
        public virtual Task<double> SummariseAsync(IEnumerable<double> metrics, CancellationToken cancellationToken = default)
        {
            Summarised = true;
            Summary = metrics.Average();
            return Task.FromResult(Summary);
        }
        public virtual Task<bool> ValidateAsync(double summary, CancellationToken cancellationToken = default)
        {
            Validated = true;
            return Task.FromResult(true);
        }
        public virtual Task CommitAsync(double summary, CancellationToken cancellationToken = default)
        {
            Committed = true;
            Summary = summary;
            return Task.CompletedTask;
        }
    }

    [Fact]
    public async Task RunAsync_GathersSummarisesAndCommits()
    {
        var gatherers = new[] { new TestGatherer(new[] { 1.0, 2.0 }), new TestGatherer(new[] { 3.0 }) };
        var summarisation = new TestSummarisation();
        var orchestrator = new PipelineOrchestrator(gatherers, summarisation);

        await orchestrator.RunAsync();

        Assert.All(gatherers, g => Assert.True(g.Called));
        Assert.True(summarisation.Summarised);
        Assert.True(summarisation.Validated);
        Assert.True(summarisation.Committed);
        Assert.Equal(2.0, summarisation.Summary);
    }

    [Fact]
    public async Task RunAsync_DoesNotCommitWhenValidationFails()
    {
        var gatherers = new[] { new TestGatherer(new[] { 1.0 }) };
        var summarisation = new TestSummarisationOverrideValidate(false);
        var orchestrator = new PipelineOrchestrator(gatherers, summarisation);

        await orchestrator.RunAsync();

        Assert.True(summarisation.Summarised);
        Assert.True(summarisation.Validated);
        Assert.False(summarisation.Committed);
    }

    private class TestSummarisationOverrideValidate : TestSummarisation
    {
        private readonly bool _validate;
        public TestSummarisationOverrideValidate(bool validate) => _validate = validate;
        public override Task<bool> ValidateAsync(double summary, CancellationToken cancellationToken = default)
        {
            Validated = true;
            return Task.FromResult(_validate);
        }
        public override Task CommitAsync(double summary, CancellationToken cancellationToken = default)
        {
            Committed = true;
            Summary = summary;
            return Task.CompletedTask;
        }
    }
}
