namespace Validation.Infrastructure.Providers;

public interface IEntityIdProvider
{
    Guid GetEntityId(object entity);
}
