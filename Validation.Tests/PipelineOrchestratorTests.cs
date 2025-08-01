using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Validation.Infrastructure.Pipeline;
using Xunit;

namespace Validation.Tests;

public class PipelineOrchestratorTests
{
    private class TestGather : IGatherService
    {
        public Task<IEnumerable<T>> GatherAsync<T>(CancellationToken ct = default)
            => Task.FromResult<IEnumerable<T>>((IEnumerable<T>)(object)new[] {1, 2});
    }

    private class TestSummarise : ISummarisationService
    {
        public Task<T> SummariseAsync<T>(IEnumerable<T> items, CancellationToken ct = default)
            => Task.FromResult((T)(object)3);
    }

    private class TestValidate : IValidationService
    {
        public bool Valid { get; set; } = true;
        public Task<bool> ValidateAsync<T>(T summary, CancellationToken ct = default)
            => Task.FromResult(Valid);
    }

    private class TestCommit : ICommitService
    {
        public int Called { get; private set; }
        public Task CommitAsync<T>(T summary, CancellationToken ct = default)
        { Called++; return Task.CompletedTask; }
    }

    [Fact]
    public async Task ExecuteAsync_ValidData_Commits()
    {
        var gather = new TestGather();
        var summarise = new TestSummarise();
        var validate = new TestValidate {Valid = true};
        var commit = new TestCommit();
        var orchestrator = new PipelineOrchestrator(gather, summarise, validate, commit);

        await orchestrator.ExecuteAsync<int>(CancellationToken.None);

        Assert.Equal(1, commit.Called);
    }

    [Fact]
    public async Task ExecuteAsync_InvalidData_DiscardEventRaised()
    {
        var gather = new TestGather();
        var summarise = new TestSummarise();
        var validate = new TestValidate {Valid = false};
        var commit = new TestCommit();
        var orchestrator = new PipelineOrchestrator(gather, summarise, validate, commit);
        bool discarded = false;
        orchestrator.Discarded += (_, _) => discarded = true;

        await orchestrator.ExecuteAsync<int>(CancellationToken.None);

        Assert.True(discarded);
        Assert.Equal(0, commit.Called);
    }
}
