using System.Collections.Generic;
using System.Threading.Tasks;
using Validation.Infrastructure.Pipeline;
using Xunit;

namespace Validation.Tests;

public class PipelineOrchestratorTests
{
    [Fact]
    public async Task RunPipeline_Commits_When_Validation_Passes()
    {
        var gatherers = new List<IMetricGatherer>
        {
            new InMemoryMetricGatherer(new[] {1m, 2m, 3m})
        };
        var summary = new InMemorySummarisationService();
        var orchestrator = new PipelineOrchestrator(gatherers, summary);

        await orchestrator.RunPipelineAsync();

        Assert.Single(orchestrator.CommittedResults);
        Assert.Equal(2m, orchestrator.CommittedResults[0]);
    }

    [Fact]
    public async Task RunPipeline_DoesNotCommit_When_Validation_Fails()
    {
        var gatherers = new List<IMetricGatherer>
        {
            new InMemoryMetricGatherer(new[] {20m})
        };
        var summary = new InMemorySummarisationService();
        var orchestrator = new PipelineOrchestrator(gatherers, summary);

        await orchestrator.RunPipelineAsync();

        Assert.Empty(orchestrator.CommittedResults);
    }
}
