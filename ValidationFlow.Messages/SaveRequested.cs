namespace ValidationFlow.Messages;

public record SaveRequested<T>(string AppName, string EntityType, Guid EntityId, T Payload);

