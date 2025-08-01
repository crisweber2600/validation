namespace Validation.Domain;

public interface IEntityIdProvider
{
    Guid GetId(object entity);
}
