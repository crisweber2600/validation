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
        var idProp = typeof(T).GetProperty("Id");
        var entityId = idProp != null && idProp.PropertyType == typeof(Guid)
            ? (Guid)(idProp.GetValue(entity) ?? Guid.NewGuid())
            : Guid.NewGuid();

        var appName = app ?? typeof(T).Assembly.GetName().Name ?? "Unknown";
        return _bus.Publish(new SaveRequested<T>(appName, typeof(T).Name, entityId, entity), ct);
    }

    public Task DeleteAsync(Guid id, string? app = null, CancellationToken ct = default)
    {
        var appName = app ?? typeof(T).Assembly.GetName().Name ?? "Unknown";
        return _bus.Publish(new DeleteRequested<T>(appName, typeof(T).Name, id), ct);
    }
}