namespace Validation.Domain.Validation;

public interface IValidationRule
{
    bool Validate(decimal previousValue, decimal newValue);
}
