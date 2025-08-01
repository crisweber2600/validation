using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging.Abstractions;
using Validation.Domain.Entities;
using Validation.Domain.Validation;
using Validation.Infrastructure.Metrics;
using Validation.Tests;
using Xunit;

public class MetricsPipelineOrchestratorTests
{
    [Fact]
    public async Task Orchestrator_runs_pipeline_and_commits_audit()
    {
        var gatherer = new InMemoryGatherer(new[] {1m, 3m});
        var summariser = new AverageSummarisationService();
        var planProvider = new InMemoryValidationPlanProvider();
        planProvider.AddPlan<Item>(new ValidationPlan(_ => 0m, ThresholdType.RawDifference, 10m));
        var validator = new SummarisationValidator();
        var repo = new InMemorySaveAuditRepository();
        var orchestrator = new PipelineOrchestrator<Item>(new[] {gatherer}, summariser, planProvider, validator, repo, NullLogger<PipelineOrchestrator<Item>>.Instance);

        var id = Guid.NewGuid();
        await orchestrator.RunAsync(id);

        var audit = repo.Audits.Single(a => a.EntityId == id);
        Assert.Equal(2m, audit.Metric);
        Assert.True(audit.IsValid);
    }
}
