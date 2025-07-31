namespace Validation.Domain.Validation;

using System.Linq;

public interface ISummarisationValidator
{
    bool Validate(decimal previousValue, decimal newValue, IEnumerable<IValidationRule> rules);
}

public class SummarisationValidator : ISummarisationValidator
{
    public bool Validate(decimal previousValue, decimal newValue, IEnumerable<IValidationRule> rules)
    {
        return rules.All(r => r.Validate(previousValue, newValue));
    }
}