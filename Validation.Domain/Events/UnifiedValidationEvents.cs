using System;

namespace Validation.Domain.Events;

/// <summary>
/// Central event hub for all validation messages and unified event handling
/// </summary>
public interface IValidationEventHub
{
    Task PublishAsync<T>(T validationEvent) where T : IValidationEvent;
    Task<IEnumerable<IValidationEvent>> GetEventsAsync(string entityType, DateTime? since = null);
}

/// <summary>
/// Base interface for all validation events to enable unified handling
/// </summary>
public interface IValidationEvent
{
    Guid EntityId { get; }
    string EntityType { get; }
    DateTime Timestamp { get; }
}

/// <summary>
/// Base interface for events that can be retried
/// </summary>
public interface IRetryableEvent : IValidationEvent
{
    int AttemptNumber { get; }
}

/// <summary>
/// Base interface for events with audit information
/// </summary>
public interface IAuditableEvent : IValidationEvent
{
    Guid? AuditId { get; }
    string? AuditDetails { get; }
}

/// <summary>
/// Enhanced delete validation event with audit support
/// </summary>
public record DeleteValidationCompleted(
    Guid EntityId, 
    string EntityType, 
    bool Validated, 
    Guid? AuditId = null,
    string? AuditDetails = null) : IAuditableEvent
{
    public DateTime Timestamp { get; } = DateTime.UtcNow;
}

/// <summary>
/// Enhanced delete rejection event with detailed information
/// </summary>
public record DeleteValidationRejected(
    Guid EntityId,
    string EntityType,
    string Reason,
    string? ValidationDetails = null,
    Guid? AuditId = null) : IAuditableEvent
{
    public DateTime Timestamp { get; } = DateTime.UtcNow;
    public string? AuditDetails => ValidationDetails;
}

/// <summary>
/// Enhanced save validation event with audit support
/// </summary>
public record SaveValidationCompleted(
    Guid EntityId,
    string EntityType,
    bool Validated,
    object? Payload = null,
    Guid? AuditId = null,
    string? AuditDetails = null) : IAuditableEvent
{
    public DateTime Timestamp { get; } = DateTime.UtcNow;
}

/// <summary>
/// Unified validation failure event for any operation type
/// </summary>
public record ValidationOperationFailed(
    Guid EntityId,
    string EntityType,
    string OperationType, // "Save", "Delete", "Update", etc.
    string Error,
    Exception? Exception = null,
    int AttemptNumber = 1) : IRetryableEvent
{
    public DateTime Timestamp { get; } = DateTime.UtcNow;
}

/// <summary>
/// Event for soft delete operations
/// </summary>
public record SoftDeleteRequested(
    Guid EntityId,
    string EntityType,
    string? DeletedBy = null,
    string? DeleteReason = null) : IValidationEvent
{
    public DateTime Timestamp { get; } = DateTime.UtcNow;
}

/// <summary>
/// Event when soft delete is completed
/// </summary>
public record SoftDeleteCompleted(
    Guid EntityId,
    string EntityType,
    DateTime DeletedAt,
    string? DeletedBy = null,
    Guid? AuditId = null) : IAuditableEvent
{
    public DateTime Timestamp { get; } = DateTime.UtcNow;
    public string? AuditDetails => $"Soft deleted at {DeletedAt} by {DeletedBy}";
}

/// <summary>
/// Event for restoring soft-deleted entities
/// </summary>
public record SoftDeleteRestored(
    Guid EntityId,
    string EntityType,
    string? RestoredBy = null,
    string? RestoreReason = null,
    Guid? AuditId = null) : IAuditableEvent
{
    public DateTime Timestamp { get; } = DateTime.UtcNow;
    public string? AuditDetails => $"Restored by {RestoredBy}: {RestoreReason}";
}