namespace Validation.Domain.Metrics;

public static class SummarisationValidator
{
    public static bool Validate<T>(double currentMetric, double previousMetric, ValidationPlan<T> plan)
    {
        return plan.Rule.Validate((decimal)previousMetric, (decimal)currentMetric);
    }
}
