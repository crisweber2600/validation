namespace Validation.Domain.Events;

public record DeleteCommitFault<T>(Guid EntityId, Guid AuditId, string Error);
