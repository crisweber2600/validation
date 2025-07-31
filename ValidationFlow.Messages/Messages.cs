namespace ValidationFlow.Messages;

[Serializable]
public record SaveRequested<T>(string AppName, string EntityType, string EntityId, T Payload);

[Serializable]
public record SaveValidated<T>(string AppName, string EntityType, string EntityId, T Payload, bool Validated);

[Serializable]
public record SaveCommitFault<T>(string AppName, string EntityType, string EntityId, T Payload, string ErrorMessage);

[Serializable]
public record DeleteRequested<T>(string AppName, string EntityType, string EntityId);

[Serializable]
public record DeleteValidated<T>(string AppName, string EntityType, string EntityId);
