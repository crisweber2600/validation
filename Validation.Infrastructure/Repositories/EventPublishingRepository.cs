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
        return _bus.Publish(new SaveRequested<T>(entity, app));
    }

    public Task DeleteAsync(Guid id, string? app = null)
    {
        return _bus.Publish(new DeleteRequested(id));
    }
}
