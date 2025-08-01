using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Validation.Infrastructure;

public interface IMetricGatherer
{
    Task<IEnumerable<decimal>> GatherAsync(CancellationToken cancellationToken = default);
}
