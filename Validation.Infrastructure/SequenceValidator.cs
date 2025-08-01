using Validation.Domain;
using Validation.Domain.Validation;
using Validation.Infrastructure.Repositories;
using Validation.Infrastructure;

namespace Validation.Domain;

public static class SequenceValidator
{
    public static async Task<bool> ValidateBatchAsync<T>(
        IEnumerable<T> items,
        Func<T, decimal> selector,
        ISaveAuditRepository repository,
        IEntityIdProvider idProvider,
        decimal threshold,
        ThresholdType thresholdType,
        SaveAudit? lastAudit,
        CancellationToken ct = default)
    {
        var list = items.ToList();
        if (list.Count == 0)
            return true;

        decimal previous = lastAudit?.Metric ?? 0m;
        if (lastAudit == null)
        {
            var firstId = idProvider.GetId(list.First());
            var prev = await repository.GetLastAsync(firstId, ct);
            previous = prev?.Metric ?? 0m;
        }

        foreach (var item in list)
        {
            var value = selector(item);
            bool valid = thresholdType switch
            {
                ThresholdType.RawDifference => Math.Abs(value - previous) <= threshold,
                ThresholdType.PercentChange => previous == 0 ? true : Math.Abs((value - previous) / previous) * 100 <= threshold,
                ThresholdType.GreaterThan => value > previous + threshold,
                ThresholdType.LessThan => value < previous - threshold,
                ThresholdType.EqualTo => value == previous,
                ThresholdType.NotEqualTo => value != previous,
                _ => true
            };
            if (!valid)
                return false;
            previous = value;
        }

        return true;
    }
}
