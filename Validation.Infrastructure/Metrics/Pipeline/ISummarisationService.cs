namespace Validation.Infrastructure.Metrics.Pipeline;

public interface ISummarisationService
{
    Task<double> SummariseAsync(IEnumerable<double> metrics, CancellationToken cancellationToken = default);
    Task<bool> ValidateAsync(double summary, CancellationToken cancellationToken = default);
    Task CommitAsync(double summary, CancellationToken cancellationToken = default);
}
