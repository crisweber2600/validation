using Validation.Domain;

namespace Validation.Infrastructure;

public class ReflectionEntityIdProvider : IEntityIdProvider
{
    public Guid GetId<T>(T entity)
    {
        if (entity == null) throw new ArgumentNullException(nameof(entity));
        var prop = typeof(T).GetProperty("Id");
        if (prop == null) throw new InvalidOperationException("Id property not found");
        return (Guid)(prop.GetValue(entity) ?? Guid.Empty);
    }
}
