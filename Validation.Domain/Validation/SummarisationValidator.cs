namespace Validation.Domain.Validation;

public class SummarisationValidator
{
    public bool Validate(IEnumerable<IValidationRule> rules, decimal previousValue, decimal newValue)
    {
        foreach (var rule in rules)
        {
            if (!rule.Validate(previousValue, newValue))
                return false;
        }
        return true;
    }
}
