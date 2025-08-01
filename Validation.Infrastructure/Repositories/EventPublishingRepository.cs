using MassTransit;
using System.Reflection;
using Validation.Domain.Events;
using ValidationFlow.Messages;
using Validation.Domain.Repositories;

namespace Validation.Infrastructure.Repositories;

public class EventPublishingRepository<T> : IEntityRepository<T>
{
    private readonly IBus _bus;
    private readonly string _appName;

    public EventPublishingRepository(IBus bus, string? appName = null)
    {
        _bus = bus;
        _appName = appName ?? Assembly.GetEntryAssembly()?.GetName().Name ?? "Unknown";
    }

    public Task SaveAsync(T entity, string? app = null, CancellationToken ct = default)
    {
        var finalApp = app ?? _appName;
        var idProp = typeof(T).GetProperty("Id");
        var entityId = idProp?.PropertyType == typeof(Guid) ? (Guid)(idProp.GetValue(entity) ?? Guid.NewGuid()) : Guid.NewGuid();
        return _bus.Publish(new SaveRequested<T>(finalApp, typeof(T).Name, entityId, entity), ct);
    }

    public Task DeleteAsync(Guid id, string? app = null, CancellationToken ct = default)
    {
        var finalApp = app ?? _appName;
        return _bus.Publish(new DeleteRequested<T>(finalApp, typeof(T).Name, id), ct);
    }
}