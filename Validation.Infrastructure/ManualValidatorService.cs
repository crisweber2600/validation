using System.Collections.Concurrent;
using System.Linq;
using Validation.Domain.Validation;

namespace Validation.Infrastructure;

public class ManualValidatorService : IManualValidatorService
{
    private readonly ConcurrentDictionary<Type, List<Func<object, bool>>> _rules = new();
    private readonly ConcurrentDictionary<Type, List<Delegate>> _typedRules = new();

    public void AddRule<T>(Func<T, bool> rule)
    {
        var list = _rules.GetOrAdd(typeof(T), _ => new List<Func<object, bool>>());
        list.Add(o => rule((T)o));

        var typed = _typedRules.GetOrAdd(typeof(T), _ => new List<Delegate>());
        typed.Add(rule);
    }

    internal bool ContainsRule<T>(Func<T, bool> rule)
    {
        return _typedRules.TryGetValue(typeof(T), out var list) && list.Contains(rule);
    }

    public IEnumerable<Func<object, bool>> GetRules(Type type)
    {
        return _rules.TryGetValue(type, out var list) ? list : Enumerable.Empty<Func<object, bool>>();
    }

    public void RemoveRules(Type type)
    {
        _rules.TryRemove(type, out _);
        _typedRules.TryRemove(type, out _);
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