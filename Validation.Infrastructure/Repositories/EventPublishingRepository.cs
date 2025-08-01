using MassTransit;
using System.Reflection;
using Validation.Domain.Repositories;
using ValidationFlow.Messages;

namespace Validation.Infrastructure.Repositories;

public class EventPublishingRepository<T> : IEntityRepository<T>
{
    private readonly IBus _bus;
    private readonly string _appName;

    public EventPublishingRepository(IBus bus, string? appName = null)
    {
        _bus = bus;
        _appName = appName ?? Assembly.GetEntryAssembly()?.GetName().Name ?? "UnknownApp";
    }

    public Task SaveAsync(T entity, string? app = null, CancellationToken ct = default)
    {
        var idProp = typeof(T).GetProperty("Id");
        var id = idProp != null ? (Guid)(idProp.GetValue(entity) ?? Guid.NewGuid()) : Guid.NewGuid();
        var message = new SaveRequested<T>(_appName, typeof(T).Name, id, entity);
        return _bus.Publish(message, ct);
    }

    public Task DeleteAsync(Guid id, string? app = null, CancellationToken ct = default)
    {
        var message = new DeleteRequested<T>(_appName, typeof(T).Name, id);
        return _bus.Publish(message, ct);
    }
}