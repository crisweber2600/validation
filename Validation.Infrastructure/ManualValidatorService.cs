using System.Collections.Concurrent;

namespace Validation.Infrastructure;

public interface IManualValidatorService
{
    bool Validate(object instance);
}

public record ManualValidationRule(Type EntityType, Func<object, bool> Rule);

public class ManualValidatorService : IManualValidatorService
{
    private readonly ConcurrentDictionary<Type, List<Func<object, bool>>> _rules = new();

    public ManualValidatorService(IEnumerable<ManualValidationRule>? rules = null)
    {
        if (rules != null)
        {
            foreach (var rule in rules)
            {
                AddRule(rule.EntityType, rule.Rule);
            }
        }
    }

    private void AddRule(Type type, Func<object, bool> rule)
    {
        var list = _rules.GetOrAdd(type, _ => new List<Func<object, bool>>());
        lock (list)
        {
            list.Add(rule);
        }
    }

    public void RegisterRule<T>(Func<T, bool> rule)
    {
        AddRule(typeof(T), o => rule((T)o));
    }

    public bool Validate(object instance)
    {
        if (_rules.TryGetValue(instance.GetType(), out var list))
        {
            foreach (var rule in list)
            {
                if (!rule(instance))
                {
                    return false;
                }
            }
        }
        return true;
    }
}
