using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Validation.Infrastructure.Repositories;
using Microsoft.Extensions.Logging;

namespace Validation.Domain.Validation;

public static class SequenceValidator
{
    public static async Task<bool> ValidateBatchAsync<T>(
        IEnumerable<T> items,
        Func<T, decimal> selector,
        ISaveAuditRepository audits,
        IEntityIdProvider ids,
        decimal thresholdValue,
        ThresholdType thresholdType,
        ILogger? logger,
        CancellationToken ct)
    {
        foreach (var item in items)
        {
            var id = ids.GetId(item);
            var last = await audits.GetLastAsync(id, ct);
            if (last == null) continue;
            var metric = selector(item);
            var previous = last.Metric;
            var valid = thresholdType switch
            {
                ThresholdType.RawDifference => Math.Abs(metric - previous) <= thresholdValue,
                ThresholdType.PercentChange => previous == 0 ? true : Math.Abs((metric - previous) / previous) * 100 <= thresholdValue,
                _ => true
            };
            if (!valid)
                return false;
        }
        return true;
    }
}

public interface IEntityIdProvider
{
    Guid GetId<T>(T entity);
}

public interface IApplicationNameProvider
{
    string ApplicationName { get; }
}
