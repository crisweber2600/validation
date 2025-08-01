namespace Validation.Domain.Validation;

public interface IManualValidatorService
{
    bool Validate(object instance);
}