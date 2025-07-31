using System.Collections.Concurrent;
using Microsoft.Extensions.Options;

namespace Validation.Infrastructure;

public class ManualValidatorService : IManualValidatorService
{
    private readonly ConcurrentDictionary<Type, List<Func<object, bool>>> _rules;

    public ManualValidatorService(IOptions<ManualValidatorOptions> options)
    {
        _rules = options.Value.Rules;
    }

    public bool Validate(object instance)
    {
        if (instance == null) return true;
        if (_rules.TryGetValue(instance.GetType(), out var list))
        {
            foreach (var rule in list)
            {
                if (!rule(instance)) return false;
            }
        }
        return true;
    }
}

public class ManualValidatorOptions
{
    internal ConcurrentDictionary<Type, List<Func<object, bool>>> Rules { get; } = new();
}
