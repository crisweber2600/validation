namespace Validation.Infrastructure.Pipeline;

public interface ISummarisationService
{
    Task<decimal> SummariseAsync<T>(IEnumerable<T> items, CancellationToken cancellationToken = default);
}
