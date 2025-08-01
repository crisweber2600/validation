using Validation.Domain;
using Validation.Domain.Validation;
using Validation.Infrastructure.Repositories;

namespace Validation.Infrastructure;

public static class SequenceValidator
{
    public static async Task<bool> ValidateAsync<T>(
        T entity,
        Func<T, decimal> selector,
        ISaveAuditRepository auditRepo,
        IEntityIdProvider idProvider,
        decimal threshold,
        ThresholdType thresholdType,
        CancellationToken ct = default)
    {
        var id = idProvider.GetId(entity);
        var last = await auditRepo.GetLastAsync(id, ct);
        var previous = last?.Metric ?? 0m;
        var metric = selector(entity);

        return thresholdType switch
        {
            ThresholdType.RawDifference => Math.Abs(metric - previous) <= threshold,
            ThresholdType.PercentChange => previous == 0
                ? true
                : Math.Abs((metric - previous) / previous) * 100 <= threshold,
            _ => true
        };
    }
}
