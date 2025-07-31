using System;
using System.Collections.Generic;
using System.Linq;

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
            double result = rule.Strategy switch
            {
                ValidationStrategy.Sum => all.Sum(ruleSet.Selector),
                ValidationStrategy.Average => all.Average(ruleSet.Selector),
                ValidationStrategy.Count => all.Count(),
                _ => double.NaN
            };

            if (rule.Strategy == ValidationStrategy.Variance)
            {
                var values = all.Select(ruleSet.Selector).ToArray();
                var avg = values.Average();
                result = values.Select(v => Math.Pow(v - avg, 2)).Average();
            }

            if (double.IsNaN(result))
                throw new ArgumentOutOfRangeException();

            if (result > rule.Threshold)
                return false;
        }

        return true;
    }
}
