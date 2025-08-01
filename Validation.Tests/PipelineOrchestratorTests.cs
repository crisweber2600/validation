using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Validation.Infrastructure;
using Validation.Infrastructure.Repositories;
using Validation.Domain.Validation;
using Xunit;

namespace Validation.Tests;

public class PipelineOrchestratorTests
{
    private class TestGatherer : IMetricGatherer
    {
        public bool Called { get; private set; }
        public decimal Value { get; set; }
        public Task<IEnumerable<decimal>> GatherAsync(CancellationToken ct)
        {
            Called = true;
            return Task.FromResult<IEnumerable<decimal>>(new[] { Value });
        }
    }

    private class TestSummarisationService : ISummarisationService
    {
        public bool Called { get; private set; }
        public decimal Summarise(IEnumerable<decimal> metrics)
        {
            Called = true;
            return metrics.Average();
        }
    }

    [Fact]
    public async Task ExecuteAsync_Gathers_Summarises_Validates_and_Commits()
    {
        var gatherer1 = new TestGatherer { Value = 10 };
        var gatherer2 = new TestGatherer { Value = 20 };
        var summariser = new TestSummarisationService();
        var repo = new InMemorySaveAuditRepository();
        var validator = new SummarisationValidator();
        var plan = new ValidationPlan(_ => 0m, ThresholdType.RawDifference, 50m);
        var orchestrator = new PipelineOrchestrator(
            new[] { gatherer1, gatherer2 }, summariser, repo, validator, plan, Guid.NewGuid());

        await orchestrator.ExecuteAsync();

        Assert.True(gatherer1.Called);
        Assert.True(gatherer2.Called);
        Assert.True(summariser.Called);
        Assert.Single(repo.Audits);
        Assert.True(repo.Audits[0].IsValid);
    }

    [Fact]
    public async Task ExecuteAsync_Uses_Previous_Audit_For_Validation()
    {
        var gatherer = new TestGatherer { Value = 20 };
        var summariser = new TestSummarisationService();
        var repo = new InMemorySaveAuditRepository();
        var validator = new SummarisationValidator();
        var plan = new ValidationPlan(_ => 0m, ThresholdType.RawDifference, 5m);
        var entityId = Guid.NewGuid();
        repo.Audits.Add(new SaveAudit { Id = Guid.NewGuid(), EntityId = entityId, Metric = 10 });
        var orchestrator = new PipelineOrchestrator(
            new[] { gatherer }, summariser, repo, validator, plan, entityId);

        await orchestrator.ExecuteAsync();

        Assert.False(repo.Audits.Last().IsValid);
    }
}
