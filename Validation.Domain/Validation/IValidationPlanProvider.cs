namespace Validation.Domain.Validation;

public interface IValidationPlanProvider
{
    ValidationPlan GetPlan(Type t);
    void AddPlan<T>(ValidationPlan plan);
}