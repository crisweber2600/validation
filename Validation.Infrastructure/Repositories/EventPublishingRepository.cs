using MassTransit;
using System.Reflection;
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

    private static string GetAppName(string? app) =>
        app ?? Assembly.GetEntryAssembly()?.GetName().Name ?? "Unknown";

    public Task SaveAsync(T entity, string? app = null, CancellationToken ct = default)
    {
        var idProp = typeof(T).GetProperty("Id");
        var id = idProp != null && idProp.PropertyType == typeof(Guid)
            ? (Guid)(idProp.GetValue(entity) ?? Guid.NewGuid())
            : Guid.NewGuid();
        return _bus.Publish(new SaveRequested<T>(GetAppName(app), typeof(T).Name, id, entity), ct);
    }

    public Task DeleteAsync(Guid id, string? app = null, CancellationToken ct = default)
    {
        return _bus.Publish(new DeleteRequested<T>(GetAppName(app), typeof(T).Name, id), ct);
    }
}