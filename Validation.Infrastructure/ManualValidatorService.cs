using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Validation.Domain.Validation;

namespace Validation.Infrastructure;

public class ManualValidatorService : IManualValidatorService
{
    private readonly ConcurrentDictionary<Type, List<Func<object, bool>>> _rules = new();
    private readonly ConcurrentDictionary<Type, List<Delegate>> _typedRules = new();

    public void AddRule<T>(Func<T, bool> rule)
    {
        var objList = _rules.GetOrAdd(typeof(T), _ => new List<Func<object, bool>>());
        var typedList = _typedRules.GetOrAdd(typeof(T), _ => new List<Delegate>());
        objList.Add(o => rule((T)o));
        typedList.Add(rule);
    }

    public bool Validate(object instance)
    {
        if (instance == null) throw new ArgumentNullException(nameof(instance));
        var type = instance.GetType();
        if (!_rules.TryGetValue(type, out var list) || list.Count == 0)
            return true;
        return list.All(r => r(instance));
    }

    public IEnumerable<Func<object, bool>> GetRules(Type type)
    {
        return _rules.TryGetValue(type, out var list) ? list.ToList() : Enumerable.Empty<Func<object, bool>>();
    }

    public void RemoveRules(Type type)
    {
        _rules.TryRemove(type, out _);
        _typedRules.TryRemove(type, out _);
    }

    internal IEnumerable<Delegate> GetTypedRules(Type type)
    {
        return _typedRules.TryGetValue(type, out var list) ? list.ToList() : Enumerable.Empty<Delegate>();
    }
}