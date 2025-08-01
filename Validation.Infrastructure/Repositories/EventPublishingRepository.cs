using MassTransit;
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
        _appName = appName ?? System.Reflection.Assembly.GetEntryAssembly()?.GetName().Name ?? "Unknown";
    }

    public Task SaveAsync(T entity, string? app = null, CancellationToken ct = default)
    {
        var appName = app ?? _appName;
        var entityId = (Guid?)entity?.GetType().GetProperty("Id")?.GetValue(entity) ?? Guid.Empty;
        return _bus.Publish(new SaveRequested<T>(appName, typeof(T).Name, entityId, entity), ct);
    }

    public Task DeleteAsync(Guid id, string? app = null, CancellationToken ct = default)
    {
        var appName = app ?? _appName;
        return _bus.Publish(new DeleteRequested<T>(appName, typeof(T).Name, id), ct);
    }
}