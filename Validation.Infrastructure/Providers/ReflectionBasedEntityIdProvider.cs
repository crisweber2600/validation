namespace Validation.Infrastructure;

public class ReflectionBasedEntityIdProvider : IEntityIdProvider
{
    private readonly string[] _priority;

    public ReflectionBasedEntityIdProvider(params string[] priority)
    {
        _priority = priority.Length > 0 ? priority : new[] { "Id", "EntityId" };
    }

    public Guid GetEntityId(object entity)
    {
        if (entity == null) throw new ArgumentNullException(nameof(entity));
        var type = entity.GetType();
        foreach (var name in _priority)
        {
            var prop = type.GetProperty(name);
            if (prop != null && prop.PropertyType == typeof(Guid))
            {
                if (prop.GetValue(entity) is Guid id)
                    return id;
            }
        }
        throw new InvalidOperationException($"No GUID Id property found on {type.FullName}");
    }
}
