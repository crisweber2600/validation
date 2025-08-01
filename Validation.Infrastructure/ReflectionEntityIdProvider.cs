using Validation.Domain;

namespace Validation.Infrastructure;

public class ReflectionEntityIdProvider : IEntityIdProvider
{
    public Guid GetId<T>(T entity)
    {
        if (entity == null) throw new ArgumentNullException(nameof(entity));
        var prop = entity.GetType().GetProperty("Id");
        if (prop == null)
            throw new InvalidOperationException("Entity does not have an Id property");
        return (Guid)(prop.GetValue(entity) ?? throw new InvalidOperationException("Id value is null"));
    }
}
