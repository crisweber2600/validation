namespace ValidationFlow.Domain;

public enum ThresholdType
{
    RawDifference,
    PercentChange
}

public record SaveAudit(
    string EntityType,
    Guid EntityId,
    decimal MetricValue,
    bool Validated,
    DateTime Timestamp);

public record ValidationPlan<T>(
    Func<T, decimal> MetricSelector,
    ThresholdType ThresholdType,
    decimal ThresholdValue);
