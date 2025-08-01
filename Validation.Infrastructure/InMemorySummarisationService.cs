using System.Collections.Generic;
using System.Linq;

namespace Validation.Infrastructure;

public class InMemorySummarisationService : ISummarisationService
{
    public decimal Summarise(IEnumerable<decimal> values)
    {
        var list = values.ToList();
        if (list.Count == 0) return 0m;
        return (decimal)list.Average();
    }
}
