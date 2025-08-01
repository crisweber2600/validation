using System;
namespace Validation.Domain.Events;

[Obsolete("Use ValidationFlow.Messages.DeleteValidated<T> instead")]
public record DeleteValidated(Guid EntityId, Guid AuditId, string EntityType);
[Obsolete("Use ValidationFlow.Messages.DeleteRejected<T> instead")]
public record DeleteRejected(Guid EntityId, Guid AuditId, string Reason, string EntityType);
public record DeleteValidationFailed(Guid EntityId, string Error, string EntityType);