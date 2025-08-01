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
        var type = typeof(T);
        var typedList = _typedRules.GetOrAdd(type, _ => new List<Delegate>());
        if (typedList.Any(d => d.Equals(rule)))
            throw new InvalidOperationException($"A rule for type '{type.Name}' with the same signature already exists.");
        typedList.Add(rule);

        var list = _rules.GetOrAdd(type, _ => new List<Func<object, bool>>());
        list.Add(o => rule((T)o));
    }

    public IEnumerable<Func<object, bool>> GetRules(Type type)
    {
        if (type == null) throw new ArgumentNullException(nameof(type));
        return _rules.TryGetValue(type, out var list) ? list.ToArray() : Enumerable.Empty<Func<object, bool>>();
    }

    public void RemoveRules(Type type)
    {
        if (type == null) throw new ArgumentNullException(nameof(type));
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