using MassTransit;
using System.Reflection;
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
        var appName = app ?? _appName;
        var id = typeof(T).GetProperty("Id")?.GetValue(entity) as Guid? ?? Guid.NewGuid();
        return _bus.Publish(new SaveRequested<T>(appName, typeof(T).Name, id, entity), ct);
    }

    public Task DeleteAsync(Guid id, string? app = null, CancellationToken ct = default)
    {
        var appName = app ?? _appName;
        return _bus.Publish(new DeleteRequested<T>(appName, typeof(T).Name, id), ct);
    }
}