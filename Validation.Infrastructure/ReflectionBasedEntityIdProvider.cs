using System.Reflection;
using Validation.Domain;

namespace Validation.Infrastructure;

public sealed class ReflectionBasedEntityIdProvider : IEntityIdProvider
{
    private readonly string[] _priority;

    public ReflectionBasedEntityIdProvider(params string[] priority)
        => _priority = priority.Length == 0
            ? new[] { "Name", "Code", "Key", "Identifier", "Title", "Label" }
            : priority;

    public string GetId<T>(T entity)
    {
        var type = entity!.GetType();
        foreach (var name in _priority)
        {
            var prop = type.GetProperty(name, BindingFlags.Public|BindingFlags.Instance|BindingFlags.IgnoreCase);
            if (prop != null && prop.PropertyType == typeof(string))
            {
                var val = (string?)prop.GetValue(entity);
                if (!string.IsNullOrWhiteSpace(val)) return val!;
            }
        }
        var idProp = type.GetProperty("Id") ?? type.GetProperty($"{type.Name}Id");
        return idProp?.GetValue(entity)?.ToString() ?? Guid.Empty.ToString();
    }
}
