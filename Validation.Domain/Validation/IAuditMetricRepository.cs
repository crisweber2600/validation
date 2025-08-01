using System.Threading;
using System.Threading.Tasks;

namespace Validation.Domain.Validation;

public interface IAuditMetricRepository
{
    Task<decimal?> GetLastMetricAsync(string id, CancellationToken ct = default);
}
