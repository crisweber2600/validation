namespace Validation.Domain.Validation;

public class ValidationPlan
{
    public IEnumerable<IValidationRule> Rules { get; }
    public Func<object, decimal>? MetricSelector { get; }
    public ThresholdType? ThresholdType { get; }
    public decimal? ThresholdValue { get; }

    // Constructor for rule-based validation
    public ValidationPlan(IEnumerable<IValidationRule> rules)
    {
        Rules = rules;
    }

    // Constructor for threshold-based validation
    public ValidationPlan(Func<object, decimal> metricSelector, ThresholdType thresholdType, decimal thresholdValue)
    {
        Rules = Enumerable.Empty<IValidationRule>();
        MetricSelector = metricSelector;
        ThresholdType = thresholdType;
        ThresholdValue = thresholdValue;
    }

    // Constructor for metric tracking without thresholds
    public ValidationPlan(Func<object, decimal> metricSelector)
    {
        Rules = Enumerable.Empty<IValidationRule>();
        MetricSelector = metricSelector;
    }
}