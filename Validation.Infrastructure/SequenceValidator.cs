using Validation.Domain;
using Validation.Domain.Validation;
using Validation.Infrastructure.Repositories;

namespace Validation.Infrastructure;

public static class SequenceValidator
{
    public static async Task<bool> ValidateAsync<T>(
        T entity,
        Func<object, decimal>? selector,
        ISaveAuditRepository repo,
        IEntityIdProvider idProvider,
        decimal threshold,
        ThresholdType type,
        CancellationToken ct = default)
    {
        if (selector == null) return true;
        var id = idProvider.GetId(entity!);
        var last = await repo.GetLastAsync(id, ct);
        if (last == null) return true;
        var current = selector(entity!);
        var previous = last.Metric;
        return type switch
        {
            ThresholdType.PercentChange => previous == 0 ? true : Math.Abs((current - previous) / previous) * 100 <= threshold,
            _ => Math.Abs(current - previous) <= threshold
        };
    }
}
