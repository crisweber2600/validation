using Validation.Domain.Validation;

namespace Validation.Infrastructure.Metrics;

public class PipelineOrchestrator
{
    private readonly IMetricsGatherer _gatherer;
    private readonly ISummarisationService _summariser;
    private readonly SummarisationValidator _validator;
    private readonly ValidationPlan _plan;
    private readonly ISummaryCommitter _committer;

    public PipelineOrchestrator(
        IMetricsGatherer gatherer,
        ISummarisationService summariser,
        SummarisationValidator validator,
        ValidationPlan plan,
        ISummaryCommitter committer)
    {
        _gatherer = gatherer;
        _summariser = summariser;
        _validator = validator;
        _plan = plan;
        _committer = committer;
    }

    public async Task RunAsync(CancellationToken cancellationToken = default)
    {
        var metrics = await _gatherer.GatherAsync(cancellationToken);
        var summary = await _summariser.SummariseAsync(metrics, cancellationToken);
        if (!_validator.Validate(0m, summary, _plan))
            throw new InvalidOperationException("Summary failed validation");
        await _committer.CommitAsync(summary, cancellationToken);
    }
}
