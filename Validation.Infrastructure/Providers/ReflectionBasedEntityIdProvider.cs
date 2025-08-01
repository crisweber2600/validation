using System.Reflection;

namespace Validation.Infrastructure.Providers;

public class ReflectionBasedEntityIdProvider : IEntityIdProvider
{
    private readonly string[] _priority;

    public ReflectionBasedEntityIdProvider(params string[] priority)
    {
        _priority = priority.Length > 0 ? priority : new[] { "Id" };
    }

    public Guid GetEntityId(object entity)
    {
        if (entity == null) throw new ArgumentNullException(nameof(entity));
        var type = entity.GetType();
        foreach (var name in _priority)
        {
            var prop = type.GetProperty(name, BindingFlags.Public | BindingFlags.Instance);
            if (prop != null && prop.PropertyType == typeof(Guid))
            {
                if (prop.GetValue(entity) is Guid id)
                    return id;
            }
        }
        throw new InvalidOperationException($"Could not find Guid Id on type {type.FullName}");
    }
}
