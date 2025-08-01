using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Validation.Infrastructure.Metrics;

public class AverageSummarisationService : ISummarisationService
{
    public Task<decimal> SummariseAsync(IEnumerable<decimal> data, CancellationToken cancellationToken = default)
    {
        var list = data.ToList();
        var avg = list.Count == 0 ? 0m : list.Average();
        return Task.FromResult(avg);
    }
}
