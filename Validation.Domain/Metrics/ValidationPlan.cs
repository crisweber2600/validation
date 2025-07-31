using System.Linq.Expressions;
using Validation.Domain.Validation;

namespace Validation.Domain.Metrics;

public class ValidationPlan<T>
{
    public ValidationStrategy Strategy { get; }
    public Expression<Func<T, double>> Selector { get; }
    public IValidationRule Rule { get; }

    public ValidationPlan(ValidationStrategy strategy, Expression<Func<T, double>> selector, IValidationRule rule)
    {
        Strategy = strategy;
        Selector = selector;
        Rule = rule;
    }
}
