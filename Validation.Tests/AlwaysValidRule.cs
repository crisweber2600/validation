using Validation.Domain.Validation;

namespace Validation.Tests;

public class AlwaysValidRule : IValidationRule
{
    public bool Validate(decimal previousValue, decimal newValue) => true;
}
