namespace Validation.Domain.Validation;

public class SummarisationValidator
{
    public bool Validate(decimal previousValue, decimal newValue, IEnumerable<IValidationRule> rules)
    {
        return rules.All(r => r.Validate(previousValue, newValue));
    }

    public bool ValidateRuleSet<T>(IQueryable<T> all, ValidationRuleSet<T> ruleSet)
    {
        var values = all.Select(ruleSet.Selector).ToList();
        var average = values.Count > 0 ? values.Average() : 0;
        foreach (var rule in ruleSet.Rules)
        {
            double summary = rule.Strategy switch
            {
                ValidationStrategy.Sum => values.Sum(),
                ValidationStrategy.Average => average,
                ValidationStrategy.Count => values.Count,
                ValidationStrategy.Variance => values.Count > 0 ? values.Select(v => Math.Pow(v - average, 2)).Average() : 0,
                _ => throw new ArgumentOutOfRangeException()
            };

            if (summary > rule.Threshold)
                return false;
        }
        return true;
    }
}
