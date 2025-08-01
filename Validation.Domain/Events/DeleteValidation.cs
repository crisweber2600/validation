using System;

namespace Validation.Domain.Events;

[Obsolete("Use ValidationFlow.Messages.DeleteValidated message instead")]
public record DeleteValidated(Guid EntityId, Guid AuditId, string EntityType);
public record DeleteRejected(Guid EntityId, Guid AuditId, string Reason, string EntityType);
public record DeleteValidationFailed(Guid EntityId, string Error, string EntityType);