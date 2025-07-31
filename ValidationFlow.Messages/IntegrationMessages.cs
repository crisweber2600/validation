namespace ValidationFlow.Messages;

[System.Serializable]
public record SaveRequested<T>(string AppName, string EntityType, Guid EntityId, T Payload);

[System.Serializable]
public record SaveValidated<T>(string AppName, string EntityType, Guid EntityId, T Payload, bool Validated);

[System.Serializable]
public record SaveCommitFault<T>(string AppName, string EntityType, Guid EntityId, T Payload, string ErrorMessage);

[System.Serializable]
public record DeleteRequested<T>(string AppName, string EntityType, Guid EntityId);

[System.Serializable]
public record DeleteValidated<T>(string AppName, string EntityType, Guid EntityId, bool Validated);
