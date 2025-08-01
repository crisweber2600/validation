using System;
namespace Validation.Domain.Events;

[Obsolete("Use ValidationFlow.Messages.SaveCommitFault instead")]
public record SaveCommitFault<T>(Guid EntityId, Guid AuditId, string Error);