using Validation.Domain.Validation;
using Validation.Infrastructure.Repositories;
using Validation.Domain;

namespace Validation.Domain;

public static class SequenceValidator
{
    public static async Task<bool> ValidateAsync<T>(
        T entity,
        Func<T, decimal> selector,
        ISaveAuditRepository repo,
        IEntityIdProvider idProvider,
        decimal threshold,
        ThresholdType thresholdType,
        CancellationToken ct = default)
    {
        var last = await repo.GetLastAsync(idProvider.GetId(entity), ct);
        var previous = last?.Metric ?? 0m;
        var metric = selector(entity);

        return thresholdType switch
        {
            ThresholdType.RawDifference => Math.Abs(metric - previous) <= threshold,
            ThresholdType.PercentChange => previous == 0 ? true : Math.Abs((metric - previous) / previous) * 100 <= threshold,
            ThresholdType.GreaterThan => metric - previous > threshold,
            ThresholdType.LessThan => metric - previous < threshold,
            ThresholdType.EqualTo => metric == previous,
            ThresholdType.NotEqualTo => metric != previous,
            _ => true
        };
    }
}
