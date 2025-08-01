namespace Validation.Domain.Validation;

public class EntityValidationRuleSet<T>
{
    public Func<T, decimal> MetricSelector { get; }
    public IReadOnlyList<IValidationRule> Rules { get; }

    public EntityValidationRuleSet(Func<T, decimal> metricSelector, params IValidationRule[] rules)
    {
        MetricSelector = metricSelector;
        Rules = rules;
    }
}
