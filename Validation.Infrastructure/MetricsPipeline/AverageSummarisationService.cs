using System;
using System.Linq;

namespace Validation.Infrastructure;

public class AverageSummarisationService : ISummarisationService
{
    public decimal Summarise(IEnumerable<decimal> metrics)
    {
        var list = metrics.ToList();
        if (!list.Any()) return 0m;
        return list.Average();
    }
}
