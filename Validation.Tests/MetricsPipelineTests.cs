using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging.Abstractions;
using Validation.Domain.Validation;
using Validation.Infrastructure;
using Validation.Infrastructure.Pipeline;
using Validation.Infrastructure.Repositories;
using Xunit;

namespace Validation.Tests;

public class MetricsPipelineTests
{
    private class TestGatherer : IGatherService
    {
        public Task<IEnumerable<decimal>> GatherAsync(CancellationToken ct) =>
            Task.FromResult<IEnumerable<decimal>>(new decimal[] {1m,2m,3m});
    }

    private class AlwaysValidValidationService : ValidationService
    {
        public AlwaysValidValidationService(ISaveAuditRepository repo)
            : base(new SummarisationValidator(), new InMemoryValidationPlanProvider(), repo) {}
        public bool NextResult { get; set; } = true;
        public override async Task<bool> ValidateAsync(decimal summary, CancellationToken ct)
        {
            return await Task.FromResult(NextResult);
        }
    }

    private class RecordingCommitService : CommitService
    {
        public decimal? Summary { get; private set; }
        public bool? Valid { get; private set; }
        public RecordingCommitService(ISaveAuditRepository repo)
            : base(repo, new NullEventPublisher()) {}
        public override Task CommitAsync(decimal summary, bool valid, CancellationToken ct)
        {
            Summary = summary; Valid = valid; return Task.CompletedTask;
        }
    }

    private class RecordingDiscardHandler : DiscardHandler
    {
        public decimal? Summary { get; private set; }
        public RecordingDiscardHandler() : base(NullLogger<DiscardHandler>.Instance, new NullEventPublisher()) {}
        public override Task HandleAsync(decimal summary, CancellationToken ct)
        {
            Summary = summary; return Task.CompletedTask;
        }
    }

    [Fact]
    public async Task Orchestrator_commits_when_valid()
    {
        var repo = new InMemorySaveAuditRepository();
        var validator = new AlwaysValidValidationService(repo) { NextResult = true };
        var commit = new RecordingCommitService(repo);
        var discard = new RecordingDiscardHandler();
        var summarizer = new SummarizationService(validator);
        var orchestrator = new PipelineOrchestrator<decimal>(new TestGatherer(), summarizer, validator, commit, discard);

        await orchestrator.ExecuteAsync(CancellationToken.None);

        Assert.Equal(6m, commit.Summary);
        Assert.True(commit.Valid);
        Assert.Null(discard.Summary);
    }

    [Fact]
    public async Task Orchestrator_discards_when_invalid()
    {
        var repo = new InMemorySaveAuditRepository();
        var validator = new AlwaysValidValidationService(repo) { NextResult = false };
        var commit = new RecordingCommitService(repo);
        var discard = new RecordingDiscardHandler();
        var summarizer = new SummarizationService(validator);
        var orchestrator = new PipelineOrchestrator<decimal>(new TestGatherer(), summarizer, validator, commit, discard);

        await orchestrator.ExecuteAsync(CancellationToken.None);

        Assert.Equal(6m, discard.Summary);
        Assert.Null(commit.Summary);
    }
}

public class SummarizationServiceTests
{
    private class DummyValidationService : ValidationService
    {
        public DummyValidationService() : base(new SummarisationValidator(), new InMemoryValidationPlanProvider(), new InMemorySaveAuditRepository()) {}
        public override Task<bool> ValidateAsync(decimal summary, CancellationToken ct) => Task.FromResult(true);
    }

    [Theory]
    [InlineData(ValidationStrategy.Sum, 6)]
    [InlineData(ValidationStrategy.Average, 2)]
    [InlineData(ValidationStrategy.Count, 3)]
    public async Task Computes_expected_summary(ValidationStrategy strategy, decimal expected)
    {
        var svc = new SummarizationService(new DummyValidationService());
        var result = await svc.SummarizeAsync(new decimal[]{1m,2m,3m}, strategy, CancellationToken.None);
        Assert.Equal(expected, result);
    }
}

