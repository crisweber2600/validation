using MassTransit;
using Validation.Domain.Events;
using Validation.Domain.Providers;
using Validation.Domain.Repositories;

namespace Validation.Infrastructure.Repositories;

public class EventPublishingRepository<T> : IEntityRepository<T>
{
    private readonly IBus _bus;
    private readonly IEntityIdProvider _entityIdProvider;
    private readonly IApplicationNameProvider _applicationNameProvider;

    public EventPublishingRepository(
        IBus bus, 
        IEntityIdProvider entityIdProvider,
        IApplicationNameProvider applicationNameProvider)
    {
        _bus = bus;
        _entityIdProvider = entityIdProvider;
        _applicationNameProvider = applicationNameProvider;
    }

    public async Task SaveAsync(T entity, string? app = null, CancellationToken ct = default)
    {
        var entityId = _entityIdProvider.GetEntityId(entity);
        var applicationName = app ?? _applicationNameProvider.GetApplicationName();
        
        // Publish save requested event
        await _bus.Publish(new SaveRequested<T>(entity, applicationName), ct);
        
        // Publish unified validation event for audit trail
        var saveEvent = new SaveValidationCompleted(
            entityId,
            typeof(T).Name,
            true, // Assume valid for save request
            entity,
            null, // AuditId will be set by consumer
            $"Save requested for {typeof(T).Name}"
        );
        
        await _bus.Publish(saveEvent, ct);
    }

    public async Task DeleteAsync(Guid id, string? app = null, CancellationToken ct = default)
    {
        var applicationName = app ?? _applicationNameProvider.GetApplicationName();
        
        // Publish delete requested event
        await _bus.Publish(new DeleteRequested(id), ct);
        
        // Publish unified validation event for audit trail
        var deleteEvent = new DeleteValidationCompleted(
            id,
            typeof(T).Name,
            true, // Assume valid for delete request
            null, // AuditId will be set by consumer
            $"Delete requested for {typeof(T).Name}"
        );
        
        await _bus.Publish(deleteEvent, ct);
    }
}