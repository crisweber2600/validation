namespace Validation.Domain.Validation;

public enum ThresholdType
{
    RawDifference,
    PercentChange
}

public record ValidationPlan(Func<object, decimal> MetricSelector,
    ThresholdType ThresholdType,
    decimal ThresholdValue);
