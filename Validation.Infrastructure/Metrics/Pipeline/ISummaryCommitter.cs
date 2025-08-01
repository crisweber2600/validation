using System.Threading;
using System.Threading.Tasks;

namespace Validation.Infrastructure.Metrics;

public interface ISummaryCommitter
{
    Task CommitAsync(decimal summary, CancellationToken cancellationToken = default);
}
