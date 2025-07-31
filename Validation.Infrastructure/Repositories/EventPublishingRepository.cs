using MassTransit;
using Validation.Domain.Events;

namespace Validation.Infrastructure.Repositories;

public class EventPublishingRepository<T> : IEntityRepository<T>
{
    private readonly IBus _bus;

    public EventPublishingRepository(IBus bus)
    {
        _bus = bus;
    }

    public Task SaveAsync(T entity, string? app = null)
    {
        var idProp = entity?.GetType().GetProperty("Id");
        if (idProp == null || idProp.PropertyType != typeof(Guid))
            throw new InvalidOperationException("Entity must have Id property of type Guid");
        var id = (Guid)(idProp.GetValue(entity)!);
        return _bus.Publish(new SaveRequested<T>(id, app));
    }

    public Task DeleteAsync(Guid id, string? app = null)
    {
        return _bus.Publish(new DeleteRequested(id));
    }
}
