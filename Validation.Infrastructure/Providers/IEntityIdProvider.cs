namespace Validation.Infrastructure;

public interface IEntityIdProvider
{
    Guid GetEntityId(object entity);
}
