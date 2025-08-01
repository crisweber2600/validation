namespace Validation.Domain.Entities;

public abstract class BaseEntity : EntityWithEvents
{
    public Guid Id { get; protected set; } = Guid.NewGuid();
    public bool Validated { get; set; } = true;
}
