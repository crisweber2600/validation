namespace Validation.Domain.Validation;

public interface IValidationPlanProvider
{
    IEnumerable<IValidationRule> GetRules<T>();
    ValidationPlan GetPlan(Type t);
    void AddPlan<T>(ValidationPlan plan);
}