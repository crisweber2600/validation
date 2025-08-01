namespace Validation.Domain;

public interface IEntityIdProvider
{
    Guid GetId<T>(T entity);
}
