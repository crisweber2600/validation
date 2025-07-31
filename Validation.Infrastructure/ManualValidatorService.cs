using System.Collections.Concurrent;

namespace Validation.Infrastructure;

public class ManualValidatorService : IManualValidatorService
{
    private readonly ConcurrentDictionary<Type, List<Func<object, bool>>> _rules = new();

    public bool Validate(object instance)
    {
        var type = instance.GetType();
        if (_rules.TryGetValue(type, out var validators))
        {
            return validators.All(v => v(instance));
        }
        return true;
    }

    public void AddRule<T>(Func<T, bool> rule)
    {
        var list = _rules.GetOrAdd(typeof(T), _ => new List<Func<object, bool>>());
        list.Add(o => rule((T)o));
    }
}
