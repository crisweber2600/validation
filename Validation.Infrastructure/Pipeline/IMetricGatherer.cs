using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Validation.Infrastructure.Pipeline;

public interface IMetricGatherer
{
    Task<IEnumerable<decimal>> GatherMetricsAsync(CancellationToken cancellationToken = default);
}
