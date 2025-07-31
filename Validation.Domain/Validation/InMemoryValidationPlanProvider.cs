using System;
using System.Collections.Concurrent;

namespace Validation.Domain.Validation;

public class InMemoryValidationPlanProvider : IValidationPlanProvider
{
    private readonly ConcurrentDictionary<Type, ValidationPlan> _plans = new();

    public ValidationPlan GetPlan(Type t)
    {
        return _plans.TryGetValue(t, out var plan)
            ? plan
            : new ValidationPlan(Array.Empty<IValidationRule>());
    }

    public void AddPlan<T>(ValidationPlan plan)
    {
        _plans[typeof(T)] = plan;
    }
}
