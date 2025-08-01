using System.Collections.Concurrent;

namespace Validation.Domain.Validation;

public class InMemoryValidationPlanProvider : IValidationPlanProvider
{
    private readonly ConcurrentDictionary<Type, ValidationPlan> _plans = new();

    public IEnumerable<IValidationRule> GetRules<T>()
    {
        var plan = GetPlan(typeof(T));
        return plan.Rules;
    }

    public ValidationPlan GetPlan(Type t)
    {
        _plans.TryGetValue(t, out var plan);
        return plan ?? new ValidationPlan(Array.Empty<IValidationRule>());
    }

    public ValidationPlan GetPlanFor<T>() => GetPlan(typeof(T));

    public void AddPlan<T>(ValidationPlan plan)
    {
        _plans[typeof(T)] = plan;
    }
}