namespace Validation.Domain.Validation;

public static class IValidationPlanProviderExtensions
{
    public static ValidationPlan GetPlanFor<T>(this IValidationPlanProvider provider)
        => provider.GetPlan(typeof(T));
}
