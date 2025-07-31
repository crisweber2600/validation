namespace ValidationFlow.Messages;

public record SaveValidated<T>(string AppName, string EntityType, Guid EntityId, T Payload, bool Validated);

