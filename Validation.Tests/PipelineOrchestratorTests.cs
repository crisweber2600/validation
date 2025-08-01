using System.Collections.Generic;
using MassTransit.Testing;
using Microsoft.Extensions.Logging.Abstractions;
using Validation.Domain.Events;
using Validation.Domain.Validation;
using Validation.Infrastructure;
using Validation.Infrastructure.Pipeline;
using Validation.Infrastructure.Repositories;

namespace Validation.Tests;

public class PipelineOrchestratorTests
{
    private class FixedGatherer : IGatherService
    {
        private readonly IEnumerable<decimal> _data;
        public FixedGatherer(IEnumerable<decimal> data) => _data = data;
        public Task<IEnumerable<decimal>> GatherAsync(CancellationToken ct) => Task.FromResult(_data);
    }

    private class PassingPlanProvider : IValidationPlanProvider
    {
        public IEnumerable<IValidationRule> GetRules<T>() => Array.Empty<IValidationRule>();
        public ValidationPlan GetPlan(Type t) => new ValidationPlan(_ => 0m, ThresholdType.RawDifference, 1000m);
        public void AddPlan<T>(ValidationPlan plan) { }
    }

    private class FailingPlanProvider : IValidationPlanProvider
    {
        public IEnumerable<IValidationRule> GetRules<T>() => Array.Empty<IValidationRule>();
        public ValidationPlan GetPlan(Type t) => new ValidationPlan(_ => 0m, ThresholdType.RawDifference, 0m);
        public void AddPlan<T>(ValidationPlan plan) { }
    }

    [Fact]
    public async Task Orchestrator_commits_when_valid()
    {
        var gather = new FixedGatherer(new[] { 1m, 2m, 3m });
        var summarizer = new SummarizationService(ValidationStrategy.Sum);
        var repo = new InMemorySaveAuditRepository();
        var validation = new ValidationService(repo, new PassingPlanProvider(), new SummarisationValidator());
        var harness = new InMemoryTestHarness();
        await harness.Start();
        var commit = new CommitService(repo, harness.Bus);
        var discard = new DiscardHandler(NullLogger<DiscardHandler>.Instance, harness.Bus);
        var orchestrator = new PipelineOrchestrator<object>(gather, summarizer, validation, commit, discard);
        try
        {
            await orchestrator.ExecuteAsync(CancellationToken.None);
            Assert.Single(repo.Audits);
            Assert.True(await harness.Published.Any<SaveValidated<object>>());
        }
        finally
        {
            await harness.Stop();
        }
    }

    [Fact]
    public async Task Orchestrator_discards_when_invalid()
    {
        var gather = new FixedGatherer(new[] { 1m, 2m, 3m });
        var summarizer = new SummarizationService(ValidationStrategy.Sum);
        var repo = new InMemorySaveAuditRepository();
        var validation = new ValidationService(repo, new FailingPlanProvider(), new SummarisationValidator());
        var harness = new InMemoryTestHarness();
        await harness.Start();
        var commit = new CommitService(repo, harness.Bus);
        var discard = new DiscardHandler(NullLogger<DiscardHandler>.Instance, harness.Bus);
        var orchestrator = new PipelineOrchestrator<object>(gather, summarizer, validation, commit, discard);
        try
        {
            await orchestrator.ExecuteAsync(CancellationToken.None);
            Assert.Empty(repo.Audits);
            Assert.True(await harness.Published.Any<SaveCommitFault<object>>());
        }
        finally
        {
            await harness.Stop();
        }
    }
}
