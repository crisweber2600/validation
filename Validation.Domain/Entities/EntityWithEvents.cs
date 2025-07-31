using System.Collections.ObjectModel;

namespace Validation.Domain.Entities;

public abstract class EntityWithEvents : IEntityWithEvents
{
    private readonly List<object> _domainEvents = new();

    public IReadOnlyCollection<object> DomainEvents => new ReadOnlyCollection<object>(_domainEvents);

    public void ClearEvents() => _domainEvents.Clear();

    public void AddEvent(object @event) => _domainEvents.Add(@event);
}
