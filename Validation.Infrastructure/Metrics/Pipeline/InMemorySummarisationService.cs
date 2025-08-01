namespace Validation.Infrastructure.Metrics.Pipeline;

public class InMemorySummarisationService : ISummarisationService
{
    public List<double> Committed { get; } = new();

    public Task<double> SummariseAsync(IEnumerable<double> metrics, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(metrics.Any() ? metrics.Average() : 0);
    }

    public Task<bool> ValidateAsync(double summary, CancellationToken cancellationToken = default)
    {
        // Simple validation: always true
        return Task.FromResult(true);
    }

    public Task CommitAsync(double summary, CancellationToken cancellationToken = default)
    {
        Committed.Add(summary);
        return Task.CompletedTask;
    }
}
