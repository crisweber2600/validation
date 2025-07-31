using System.Linq.Expressions;

namespace Validation.Domain.Validation;

public class ValidationRuleSet<T>
{
    public Expression<Func<T, double>> Selector { get; }
    public ValidationRule[] Rules { get; }

    public ValidationRuleSet(Expression<Func<T, double>> selector, params ValidationRule[] rules)
    {
        Selector = selector;
        Rules = rules;
    }
}
