using System.Threading;
using System.Threading.Tasks;
using Validation.Domain;
using Validation.Infrastructure.Repositories;
using Validation.Domain.Validation;

namespace Validation.Infrastructure;

public static class SequenceValidator
{
    public static Task<bool> ValidateBatchAsync<T>(
        IEnumerable<T> entities,
        Func<T, decimal> selector,
        ISaveAuditRepository audits,
        IEntityIdProvider ids,
        decimal threshold,
        ThresholdType type,
        decimal? lastMetric,
        CancellationToken ct)
    {
        // Simple placeholder: always return true
        return Task.FromResult(true);
    }
}
