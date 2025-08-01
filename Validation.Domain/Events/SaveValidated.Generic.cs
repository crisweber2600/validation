using System;

namespace Validation.Domain.Events;

[Obsolete("Use ValidationFlow.Messages.SaveValidated message instead")]
public record SaveValidated<T>(Guid EntityId, Guid AuditId);