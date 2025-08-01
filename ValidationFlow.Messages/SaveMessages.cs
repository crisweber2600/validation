using System;
namespace ValidationFlow.Messages;

/// <summary>
/// Request to persist an entity payload.
/// </summary>
[Serializable]
public sealed record SaveRequested<T>(string AppName, string EntityType, Guid EntityId, T Payload);

/// <summary>
/// Notification that a save request has been validated.
/// </summary>
[Serializable]
public sealed record SaveValidated<T>(string AppName, string EntityType, Guid EntityId, T Payload, bool Validated);

/// <summary>
/// Notification that committing a save request failed.
/// </summary>
[Serializable]
public sealed record SaveCommitFault<T>(string AppName, string EntityType, Guid EntityId, T Payload, string ErrorMessage);

/// <summary>
/// Request to delete an entity.
/// </summary>
[Serializable]
public sealed record DeleteRequested<T>(string AppName, string EntityType, Guid EntityId);

/// <summary>
/// Notification that a delete request has been validated.
/// </summary>
[Serializable]
public sealed record DeleteValidated<T>(string AppName, string EntityType, Guid EntityId, bool Validated);

/// <summary>
/// Notification that a delete request was rejected during validation.
/// </summary>
[Serializable]
public sealed record DeleteRejected<T>(string AppName, string EntityType, Guid EntityId, string Reason);

/// <summary>
/// Notification that committing a delete request failed.
/// </summary>
[Serializable]
public sealed record DeleteCommitFault<T>(string AppName, string EntityType, Guid EntityId, string ErrorMessage);

/// <summary>
/// Request to commit a previously validated save operation.
/// </summary>
[Serializable]
public sealed record SaveCommitRequested<T>(string AppName, string EntityType, Guid EntityId, T Payload, Guid ValidationId);

/// <summary>
/// Notification that a save commit operation completed successfully.
/// </summary>
[Serializable]
public sealed record SaveCommitCompleted<T>(string AppName, string EntityType, Guid EntityId, T Payload, Guid ValidationId);

/// <summary>
/// Request to commit a previously validated delete operation.
/// </summary>
[Serializable]
public sealed record DeleteCommitRequested<T>(string AppName, string EntityType, Guid EntityId, Guid ValidationId);

/// <summary>
/// Notification that a delete commit operation completed successfully.
/// </summary>
[Serializable]
public sealed record DeleteCommitCompleted<T>(string AppName, string EntityType, Guid EntityId, Guid ValidationId);