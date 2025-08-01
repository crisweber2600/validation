using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Validation.Infrastructure.Pipeline;

public class InMemorySummarisationService : ISummarisationService
{
    public Task<decimal> SummariseAsync(IEnumerable<decimal> metrics, CancellationToken cancellationToken = default)
    {
        var data = metrics.ToList();
        decimal result = data.Count == 0 ? 0m : data.Average();
        return Task.FromResult(result);
    }
}
