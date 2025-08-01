namespace Validation.Domain.Events;

/// <summary>
/// Published when the audit repository fails to remove the audit record during
/// delete commit processing.
/// </summary>
public record DeleteCommitFault<T>(Guid EntityId, Guid AuditId, string Error);
