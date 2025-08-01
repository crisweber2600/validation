using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Validation.Domain;
using Validation.Domain.Validation;
using Validation.Infrastructure.Repositories;

namespace Validation.Infrastructure;

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
        ISaveAuditRepository auditRepo,
        IEntityIdProvider idProvider,
        decimal threshold,
        ThresholdType type,
        CancellationToken ct)
    {
        var keyString = idProvider.GetId(entity);
        var key       = Guid.Parse(keyString);
        var last      = await auditRepo.GetLastAsync(key, ct);
        var prevValue = last?.Metric ?? 0m;
        return Compare(prevValue, metric(entity), threshold, type);
    }

    public static async Task<bool> ValidateBatchAsync<T>(
        IEnumerable<T> items,
        Func<T, decimal> metric,
        ISaveAuditRepository auditRepo,
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
            var keyString = keySelector != null ? keySelector(item) : idProvider.GetId(item);
            var key       = Guid.Parse(keyString);

            if (!history.TryGetValue(keyString, out var prev))
            {
                prev = (await auditRepo.GetLastAsync(key, ct))?.Metric ?? 0m;
            }

            if (!Compare(prev, metric(item), threshold, type))
                return false;

            history[keyString] = metric(item);
        }

        return true;
    }
}
