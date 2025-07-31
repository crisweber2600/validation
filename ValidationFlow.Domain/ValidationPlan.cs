namespace ValidationFlow.Domain;

public record ValidationPlan<T>(
    Func<T, decimal> MetricSelector,
    ThresholdType ThresholdType,
    decimal ThresholdValue);
