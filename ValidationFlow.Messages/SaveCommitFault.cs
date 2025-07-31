namespace ValidationFlow.Messages;

public record SaveCommitFault<T>(string AppName, string EntityType, Guid EntityId, T Payload, string ErrorMessage);

