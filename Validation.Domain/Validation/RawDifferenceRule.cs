namespace Validation.Domain.Validation;

public class RawDifferenceRule : IValidationRule
{
    private readonly decimal _threshold;

    public RawDifferenceRule(decimal threshold)
    {
        _threshold = threshold;
    }

    public bool Validate(decimal previousValue, decimal newValue)
    {
        return Math.Abs(newValue - previousValue) <= _threshold;
    }
}
