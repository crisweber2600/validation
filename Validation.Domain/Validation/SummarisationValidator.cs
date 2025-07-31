using System;
using System.Collections.Generic;
namespace Validation.Domain.Validation;

public class SummarisationValidator
{
    public bool Validate(decimal previousValue, decimal newValue, IEnumerable<IValidationRule> rules)
    {
        return rules.All(r => r.Validate(previousValue, newValue));
    }

    public bool Validate(decimal previousValue, decimal newValue, ValidationPlan plan)
    {
        return plan.ThresholdType switch
        {
            ThresholdType.RawDifference => Math.Abs(newValue - previousValue) <= plan.ThresholdValue,
            ThresholdType.PercentChange => previousValue == 0 ? true : Math.Abs((newValue - previousValue) / previousValue) * 100 <= plan.ThresholdValue,
            _ => throw new NotSupportedException($"Unsupported threshold type {plan.ThresholdType}")
        };
    }
}
