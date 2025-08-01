using MassTransit;
using ValidationFlow.Messages;
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
        var appName = app ?? typeof(T).Assembly.GetName().Name ?? string.Empty;
        var entityType = typeof(T).Name;
        var idProp = typeof(T).GetProperty("Id");
        var entityId = idProp != null ? (Guid)(idProp.GetValue(entity) ?? Guid.Empty) : Guid.Empty;
        return _bus.Publish(new SaveRequested<T>(appName, entityType, entityId, entity), ct);
    }

    public Task DeleteAsync(Guid id, string? app = null, CancellationToken ct = default)
    {
        var appName = app ?? typeof(T).Assembly.GetName().Name ?? string.Empty;
        var entityType = typeof(T).Name;
        return _bus.Publish(new DeleteRequested<T>(appName, entityType, id), ct);
    }
}