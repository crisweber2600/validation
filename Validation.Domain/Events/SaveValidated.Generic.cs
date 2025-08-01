namespace Validation.Domain.Events;

[Obsolete("Use ValidationFlow.Messages.SaveValidated<T> instead")]
public record SaveValidated<T>(Guid EntityId, Guid AuditId);