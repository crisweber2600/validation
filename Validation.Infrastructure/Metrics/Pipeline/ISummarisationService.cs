using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Validation.Infrastructure.Metrics;

public interface ISummarisationService
{
    Task<decimal> SummariseAsync(IEnumerable<decimal> data, CancellationToken cancellationToken = default);
}
