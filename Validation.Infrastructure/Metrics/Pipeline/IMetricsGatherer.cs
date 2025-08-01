using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Validation.Infrastructure.Metrics;

public interface IMetricsGatherer
{
    Task<IEnumerable<decimal>> GatherAsync(CancellationToken cancellationToken = default);
}
