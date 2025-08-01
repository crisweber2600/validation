using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Validation.Infrastructure.Metrics;

public class InMemorySummaryCommitter : ISummaryCommitter
{
    public List<decimal> Committed { get; } = new();

    public Task CommitAsync(decimal summary, CancellationToken cancellationToken = default)
    {
        Committed.Add(summary);
        return Task.CompletedTask;
    }
}
