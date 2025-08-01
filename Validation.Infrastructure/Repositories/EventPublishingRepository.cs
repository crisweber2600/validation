using MassTransit;
using Validation.Domain.Events;
using ValidationFlow.Messages;
using System.Reflection;
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
        var idProp = typeof(T).GetProperty("Id");
        var id = idProp != null && idProp.PropertyType == typeof(Guid)
            ? (Guid)(idProp.GetValue(entity) ?? Guid.Empty)
            : Guid.Empty;
        var appName = app ?? _appName;
        return _bus.Publish(new ValidationFlow.Messages.SaveRequested<T>(appName, typeof(T).Name, id, entity), ct);
    }

    public Task DeleteAsync(Guid id, string? app = null, CancellationToken ct = default)
    {
        var appName = app ?? _appName;
        return _bus.Publish(new ValidationFlow.Messages.DeleteRequested<T>(appName, typeof(T).Name, id), ct);
    }
}