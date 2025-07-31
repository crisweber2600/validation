using System.Linq.Expressions;

namespace Validation.Domain.Validation;

public class ValidationPlan<T>
{
    public Expression<Func<T, double>> MetricSelector { get; }
    public ValidationStrategy Strategy { get; }
    public IValidationRule Rule { get; }

    public ValidationPlan(Expression<Func<T, double>> selector, ValidationStrategy strategy, IValidationRule rule)
    {
        MetricSelector = selector;
        Strategy = strategy;
        Rule = rule;
    }
}
