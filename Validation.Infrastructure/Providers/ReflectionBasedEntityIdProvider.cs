using System.Reflection;

namespace Validation.Infrastructure;

public class ReflectionBasedEntityIdProvider : IEntityIdProvider
{
    private readonly string[] _priorityProperties;

    public ReflectionBasedEntityIdProvider(params string[] priorityProperties)
    {
        _priorityProperties = priorityProperties.Length > 0 ? priorityProperties : new[] { "Id" };
    }

    public Guid GetEntityId<T>(T entity)
    {
        if (entity == null) throw new ArgumentNullException(nameof(entity));

        var type = entity!.GetType();
        foreach (var name in _priorityProperties)
        {
            var prop = type.GetProperty(name, BindingFlags.Public | BindingFlags.Instance);
            if (prop != null && prop.PropertyType == typeof(Guid))
            {
                if (prop.GetValue(entity) is Guid value && value != Guid.Empty)
                    return value;
            }
        }
        return Guid.Empty;
    }
}
