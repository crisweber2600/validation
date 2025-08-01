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

    public bool Validate(decimal previousValue, decimal newValue, ValidationPlan plan)
    {
        // If the plan has rules, use rule-based validation
        if (plan.Rules.Any())
        {
            return Validate(previousValue, newValue, plan.Rules);
        }

        // If the plan has threshold configuration, use threshold-based validation
        if (plan.ThresholdType.HasValue && plan.ThresholdValue.HasValue)
        {
            return plan.ThresholdType.Value switch
            {
                Domain.Validation.ThresholdType.RawDifference => Math.Abs(newValue - previousValue) <= plan.ThresholdValue.Value,
                Domain.Validation.ThresholdType.PercentChange => previousValue == 0
                    ? true
                    : Math.Abs((newValue - previousValue) / previousValue) * 100 <= plan.ThresholdValue.Value,
                _ => throw new ArgumentOutOfRangeException(nameof(plan.ThresholdType), plan.ThresholdType, null)
            };
        }

        return true; // Default to valid if no rules or thresholds configured
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

    public bool Validate<TItem, TKey>(IEnumerable<TItem> items,
        Func<TItem, TKey> keySelector,
        IEnumerable<IListValidationRule<TItem, TKey>> rules)
    {
        var groups = items.GroupBy(keySelector);
        foreach (var group in groups)
        {
            foreach (var rule in rules)
            {
                if (!rule.Validate(group))
                    return false;
            }
        }

        return true;
    }
}