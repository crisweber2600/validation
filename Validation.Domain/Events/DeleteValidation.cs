namespace Validation.Domain.Events;

[System.Obsolete("Use ValidationFlow.Messages.DeleteValidated<T> and other messages instead")]
public record DeleteValidated(Guid EntityId, Guid AuditId, string EntityType);
[System.Obsolete("Use ValidationFlow.Messages.DeleteRejected<T> instead")]
public record DeleteRejected(Guid EntityId, Guid AuditId, string Reason, string EntityType);
[System.Obsolete("Use ValidationFlow.Messages.DeleteValidationFailed<T> instead")]
public record DeleteValidationFailed(Guid EntityId, string Error, string EntityType);