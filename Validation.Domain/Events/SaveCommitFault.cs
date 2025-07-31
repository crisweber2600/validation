namespace Validation.Domain.Events;

public record SaveCommitFault<T>(Guid EntityId, Guid AuditId, string Error);
