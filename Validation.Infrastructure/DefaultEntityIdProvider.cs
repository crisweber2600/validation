using Validation.Domain;

namespace Validation.Infrastructure;

public class DefaultEntityIdProvider : IEntityIdProvider
{
    public Guid GetId<T>(T entity)
    {
        if (entity == null) throw new ArgumentNullException(nameof(entity));
        var prop = entity.GetType().GetProperty("Id");
        if (prop == null || prop.PropertyType != typeof(Guid))
            throw new InvalidOperationException("Entity must have Guid Id property");
        return (Guid)(prop.GetValue(entity) ?? Guid.Empty);
    }
}
