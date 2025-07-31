using MassTransit;
using Validation.Domain.Events;
using Validation.Domain.Repositories;

namespace Validation.Infrastructure.Repositories;

public class EventPublishingRepository<T> : IEntityRepository<T>
{
    private readonly IBus _bus;

    public EventPublishingRepository(IBus bus)
    {
        _bus = bus;
    }

    public Task SaveAsync(T entity, string? app = null, CancellationToken ct = default)
    {
        return _bus.Publish(new SaveRequested<T>(entity, app), ct);
    }

    public Task DeleteAsync(Guid id, string? app = null, CancellationToken ct = default)
    {
        return _bus.Publish(new DeleteRequested(id), ct);
    }
}