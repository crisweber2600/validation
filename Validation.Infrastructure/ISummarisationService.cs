using System.Collections.Generic;

namespace Validation.Infrastructure;

public interface ISummarisationService
{
    decimal Summarise(IEnumerable<decimal> values);
}
