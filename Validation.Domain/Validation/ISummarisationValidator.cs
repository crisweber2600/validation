namespace Validation.Domain.Validation;

public interface ISummarisationValidator
{
    bool Validate(decimal previousValue, decimal newValue, IEnumerable<IValidationRule> rules);
}
