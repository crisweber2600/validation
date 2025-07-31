namespace Validation.Infrastructure;

public interface IManualValidatorService
{
    bool Validate(object instance);
}
