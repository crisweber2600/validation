using System;

namespace Validation.Domain.Validation;

[Obsolete("Use ValidationPlan with ThresholdType instead.")]
public class PercentChangeRule : IValidationRule
{
    private readonly decimal _percent;

    public PercentChangeRule(decimal percent)
    {
        _percent = percent;
    }

    public bool Validate(decimal previousValue, decimal newValue)
    {
        if (previousValue == 0) return true; // avoid division by zero
        return Math.Abs((newValue - previousValue) / previousValue) * 100 <= _percent;
    }
}
