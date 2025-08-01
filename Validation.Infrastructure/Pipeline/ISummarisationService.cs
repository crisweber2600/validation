namespace Validation.Infrastructure.Pipeline;

public interface ISummarisationService
{
    Task<double> SummariseAsync(IEnumerable<double> values, CancellationToken cancellationToken = default);
}
