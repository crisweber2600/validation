namespace Validation.Domain.Events;

public record DeleteValidated(Guid EntityId, Guid AuditId, string EntityType);
public record DeleteRejected(Guid EntityId, Guid AuditId, string Reason, string EntityType);
public record DeleteValidationFailed(Guid EntityId, string Error, string EntityType);
public record DeleteCommitFault(Guid EntityId, Guid AuditId, string Error, string EntityType);
