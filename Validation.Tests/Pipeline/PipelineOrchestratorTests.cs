using Validation.Domain.Entities;
using MassTransit.Testing;
using Microsoft.Extensions.Logging.Abstractions;
using Validation.Domain.Events;
using Validation.Domain.Validation;
using Validation.Infrastructure.Pipeline;
using Validation.Infrastructure.Repositories;

namespace Validation.Tests.Pipeline;

public class PipelineOrchestratorTests
{
    private class TestGatherer : IGatherService
    {
        private readonly IEnumerable<decimal> _values;
        public TestGatherer(params decimal[] values) => _values = values;
        public Task<IEnumerable<decimal>> GatherAsync(CancellationToken ct) => Task.FromResult(_values);
    }

    [Fact]
    public async Task Orchestrator_commits_when_valid()
    {
        var harness = new InMemoryTestHarness();
        await harness.Start();
        try
        {
            var repo = new InMemorySaveAuditRepository();
            var gatherer = new TestGatherer(1m, 2m, 3m);
            var planProvider = new InMemoryValidationPlanProvider();
            planProvider.AddPlan<Item>(new ValidationPlan(o => 0m, ThresholdType.RawDifference, 10m));
            var validator = new ValidationService(repo, planProvider, new SummarisationValidator());
            var summariser = new SummarizationService(ValidationStrategy.Sum);
            var commit = new CommitService(repo, harness.Bus);
            var discard = new DiscardHandler(NullLogger<DiscardHandler>.Instance, harness.Bus);
            var orchestrator = new PipelineOrchestrator<Item>(gatherer, summariser, validator, commit, discard);

            await orchestrator.ExecuteAsync(CancellationToken.None);

            Assert.Single(repo.Audits);
            Assert.True(await harness.Published.Any<SaveValidated<Item>>());
        }
        finally
        {
            await harness.Stop();
        }
    }

    [Fact]
    public async Task Orchestrator_discards_when_invalid()
    {
        var harness = new InMemoryTestHarness();
        await harness.Start();
        try
        {
            var repo = new InMemorySaveAuditRepository();
            var gatherer = new TestGatherer(5m, 5m);
            var planProvider = new InMemoryValidationPlanProvider();
            planProvider.AddPlan<Item>(new ValidationPlan(o => 0m, ThresholdType.RawDifference, 1m));
            var validator = new ValidationService(repo, planProvider, new SummarisationValidator());
            var summariser = new SummarizationService(ValidationStrategy.Sum);
            var commit = new CommitService(repo, harness.Bus);
            var discard = new DiscardHandler(NullLogger<DiscardHandler>.Instance, harness.Bus);
            var orchestrator = new PipelineOrchestrator<Item>(gatherer, summariser, validator, commit, discard);

            await orchestrator.ExecuteAsync(CancellationToken.None);

            Assert.Empty(repo.Audits);
            Assert.False(await harness.Published.Any<SaveValidated<Item>>());
        }
        finally
        {
            await harness.Stop();
        }
    }
}
