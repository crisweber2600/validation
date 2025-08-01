using System.Collections.Concurrent;
using Validation.Domain.Validation;

namespace Validation.Infrastructure;

public class ManualValidatorService : IManualValidatorService
{
    private readonly ConcurrentDictionary<Type, List<Func<object, bool>>> _rules = new();

    public void AddRule<T>(Func<T, bool> rule)
    {
        var list = _rules.GetOrAdd(typeof(T), _ => new List<Func<object, bool>>());
        list.Add(o => rule((T)o));
    }

    public bool Validate(object instance)
    {
        if (instance == null) throw new ArgumentNullException(nameof(instance));
        var type = instance.GetType();
        if (!_rules.TryGetValue(type, out var list) || list.Count == 0)
            return true;
        return list.All(r => r(instance));
    }
}