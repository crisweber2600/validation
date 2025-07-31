using System.Linq.Expressions;

namespace Validation.Domain.Validation;

public class ValidationPlan<T>
{
    public Expression<Func<T, double>> Selector { get; }
    public ValidationStrategy Strategy { get; }
    public IValidationRule Rule { get; }

    public ValidationPlan(Expression<Func<T, double>> selector, ValidationStrategy strategy, IValidationRule rule)
    {
        Selector = selector;
        Strategy = strategy;
        Rule = rule;
    }
}
