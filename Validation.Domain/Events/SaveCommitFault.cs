namespace Validation.Domain.Events;

[System.Obsolete("Use ValidationFlow.Messages.SaveCommitFault<T> instead")]
public record SaveCommitFault<T>(Guid EntityId, Guid AuditId, string Error);