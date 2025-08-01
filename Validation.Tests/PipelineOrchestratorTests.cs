using Microsoft.Extensions.Logging.Abstractions;
using Validation.Infrastructure.Pipeline;
using Validation.Infrastructure.Metrics;

namespace Validation.Tests;

public class PipelineOrchestratorTests
{
    private class FakeGather : IGatherService
    {
        public Task<IEnumerable<double>> GatherAsync(CancellationToken cancellationToken = default)
            => Task.FromResult<IEnumerable<double>>(new[] { 1.0, 2.0 });
    }

    private class FakeSummariser : ISummarisationService
    {
        public Task<double> SummariseAsync(IEnumerable<double> values, CancellationToken cancellationToken = default)
            => Task.FromResult(values.Average());
    }

    private class FakeValidation : IValidationService
    {
        private readonly bool _result;
        public FakeValidation(bool result) => _result = result;
        public bool Validate(double value) => _result;
    }

    private class FakeCommit : ICommitService
    {
        public bool Committed { get; private set; }
        public Task CommitAsync(double value, CancellationToken cancellationToken = default)
        {
            Committed = true;
            return Task.CompletedTask;
        }
    }

    [Fact]
    public async Task ExecuteAsync_ValidData_Commits()
    {
        var commit = new FakeCommit();
        var orchestrator = new PipelineOrchestrator(
            new FakeGather(), new FakeSummariser(), new FakeValidation(true), commit,
            new MetricsCollector(NullLogger<MetricsCollector>.Instance),
            NullLogger<PipelineOrchestrator>.Instance);

        await orchestrator.ExecuteAsync();

        Assert.True(commit.Committed);
    }

    [Fact]
    public async Task ExecuteAsync_InvalidData_RaisesDiscard()
    {
        var commit = new FakeCommit();
        var orchestrator = new PipelineOrchestrator(
            new FakeGather(), new FakeSummariser(), new FakeValidation(false), commit,
            new MetricsCollector(NullLogger<MetricsCollector>.Instance),
            NullLogger<PipelineOrchestrator>.Instance);

        var discarded = false;
        orchestrator.Discarded += _ => discarded = true;
        await orchestrator.ExecuteAsync();

        Assert.False(commit.Committed);
        Assert.True(discarded);
    }
}
