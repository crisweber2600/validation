using System.Collections.Generic;

namespace Validation.Infrastructure.Metrics;

public interface ISummarisationService
{
    decimal Summarise(IEnumerable<decimal> metrics);
}
