using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Validation.Domain.Validation;

public static class SequenceValidator
{
    private static bool Compare(decimal prev, decimal current, decimal threshold, ThresholdType type)
    {
        var diff = Math.Abs(current - prev);
        var pct  = prev == 0 ? 0 : diff / prev * 100m;
        return type switch
        {
            ThresholdType.RawDifference => diff <= threshold,
            ThresholdType.PercentChange => pct  <= threshold,
            _                           => throw new ArgumentOutOfRangeException(nameof(type))
        };
    }

    public static async Task<bool> ValidateAsync<T>(
        T entity,
        Func<T, decimal> metric,
        IAuditMetricRepository auditRepo,
        IEntityIdProvider idProvider,
        decimal threshold,
        ThresholdType type,
        CancellationToken ct)
    {
        var key       = idProvider.GetId(entity);
        var prevValue = await auditRepo.GetLastMetricAsync(key, ct) ?? 0m;
        return Compare(prevValue, metric(entity), threshold, type);
    }

    public static async Task<bool> ValidateBatchAsync<T>(
        IEnumerable<T> items,
        Func<T, decimal> metric,
        IAuditMetricRepository auditRepo,
        IEntityIdProvider idProvider,
        decimal threshold,
        ThresholdType type,
        Func<T, string>? keySelector,
        CancellationToken ct)
    {
        var list    = items.ToList();
        var history = new Dictionary<string, decimal>();

        foreach (var item in list)
        {
            var key = keySelector != null ? keySelector(item) : idProvider.GetId(item);

            if (!history.TryGetValue(key, out var prev))
            {
                prev = await auditRepo.GetLastMetricAsync(key, ct) ?? 0m;
            }

            if (!Compare(prev, metric(item), threshold, type))
                return false;

            history[key] = metric(item);
        }
        return true;
    }
}
