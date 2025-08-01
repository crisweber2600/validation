using System;
using System.Linq;

namespace Validation.Infrastructure;

public class ReflectionBasedEntityIdProvider : IEntityIdProvider
{
    private readonly string[] _priority;

    public ReflectionBasedEntityIdProvider(params string[] priority)
    {
        _priority = priority?.Length > 0 ? priority : new[] { "Id" };
    }

    public Guid GetId<T>(T entity)
    {
        if (entity == null) throw new ArgumentNullException(nameof(entity));
        var type = entity.GetType();

        foreach (var name in _priority)
        {
            var prop = type.GetProperty(name);
            if (prop != null && prop.PropertyType == typeof(Guid))
            {
                var value = prop.GetValue(entity);
                if (value is Guid id)
                    return id;
            }
        }

        var fallback = type.GetProperties()
            .FirstOrDefault(p => p.PropertyType == typeof(Guid) && p.Name.EndsWith("Id", StringComparison.OrdinalIgnoreCase));
        if (fallback != null && fallback.GetValue(entity) is Guid fallbackId)
            return fallbackId;

        throw new InvalidOperationException("Could not determine entity id using reflection");
    }
}
