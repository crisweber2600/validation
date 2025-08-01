using System.Collections.Generic;
using System.Linq;

namespace Validation.Infrastructure.Metrics;

public class AverageSummarisationService : ISummarisationService
{
    public decimal Summarise(IEnumerable<decimal> metrics)
    {
        var list = metrics.ToList();
        return list.Count == 0 ? 0 : list.Average();
    }
}
