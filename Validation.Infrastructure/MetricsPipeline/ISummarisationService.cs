namespace Validation.Infrastructure;

public interface ISummarisationService
{
    decimal Summarise(IEnumerable<decimal> metrics);
}
