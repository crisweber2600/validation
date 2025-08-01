namespace Validation.Domain.Events;

/// <summary>
/// Notification that a delete request passed validation and should be committed.
/// </summary>
public record DeleteValidated<T>(Guid EntityId, Guid AuditId);
