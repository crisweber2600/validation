namespace Validation.Domain.Validation;

public class SummarisationValidator
{
    public bool Validate(decimal previousValue, decimal newValue, IEnumerable<IValidationRule> rules)
    {
        return rules.All(r => r.Validate(previousValue, newValue));
    }

    public bool ValidateRuleSet<T>(IQueryable<T> all, ValidationRuleSet<T> ruleSet)
    {
        foreach (var rule in ruleSet.Rules)
        {
            var values = all.Select(ruleSet.Selector);
            double summary = rule.Strategy switch
            {
                ValidationStrategy.Sum => values.Sum(),
                ValidationStrategy.Average => values.Average(),
                ValidationStrategy.Count => values.Count(),
                ValidationStrategy.Variance =>
                    values.Any() ? values.Select(v => Math.Pow(v - values.Average(), 2)).Average() : 0,
                _ => throw new ArgumentOutOfRangeException()
            };

            if (summary > rule.Threshold)
                return false;
        }

        return true;
    }
}