namespace Validation.Domain.Entities;

public interface IEntityWithEvents
{
    IReadOnlyCollection<object> DomainEvents { get; }
    void ClearEvents();
    void AddEvent(object @event);
}
