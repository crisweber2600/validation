namespace Validation.Domain.Validation;

public record ValidationPlan(Func<object, decimal> MetricSelector,
                             ThresholdType ThresholdType,
                             decimal ThresholdValue);
