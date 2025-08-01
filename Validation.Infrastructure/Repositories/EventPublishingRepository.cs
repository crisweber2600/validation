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
        var id = (Guid?)entity?.GetType().GetProperty("Id")?.GetValue(entity) ?? Guid.Empty;
        var appName = app ?? typeof(T).Assembly.GetName().Name ?? "App";
        return _bus.Publish(new SaveRequested<T>(appName, typeof(T).Name, id, entity), ct);
    }

    public Task DeleteAsync(Guid id, string? app = null, CancellationToken ct = default)
    {
        var appName = app ?? typeof(T).Assembly.GetName().Name ?? "App";
        return _bus.Publish(new DeleteRequested<T>(appName, typeof(T).Name, id), ct);
    }
}