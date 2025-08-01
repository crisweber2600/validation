namespace Validation.Domain.Validation;

public static class ValidationPlanProviderExtensions
{
    public static ValidationPlan GetPlanFor<T>(this IValidationPlanProvider provider)
        => provider.GetPlan(typeof(T));
}
