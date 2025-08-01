namespace Validation.Domain;

public interface IEntityIdProvider
{
    string GetId<T>(T entity);
}
