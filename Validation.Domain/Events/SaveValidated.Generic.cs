namespace Validation.Domain.Events;

public record SaveValidated<T>(Guid EntityId, Guid AuditId);