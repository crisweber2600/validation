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
public sealed record DeleteValidated<T>(string AppName, string EntityType, Guid EntityId);

/// <summary>
/// Notification that delete validation failed.
/// </summary>
[Serializable]
public sealed record DeleteValidationFailed<T>(string AppName, string EntityType, Guid EntityId, string ErrorMessage);

/// <summary>
/// Notification that a delete request was rejected.
/// </summary>
[Serializable]
public sealed record DeleteRejected<T>(string AppName, string EntityType, Guid EntityId, string Reason);