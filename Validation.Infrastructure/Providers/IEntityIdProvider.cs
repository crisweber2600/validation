namespace Validation.Infrastructure;

public interface IEntityIdProvider
{
    Guid GetEntityId<T>(T entity);
}
