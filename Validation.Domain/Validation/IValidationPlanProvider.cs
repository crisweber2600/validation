namespace Validation.Domain.Validation;

public interface IValidationPlanProvider
{
    IEnumerable<IValidationRule> GetRules<T>();
}