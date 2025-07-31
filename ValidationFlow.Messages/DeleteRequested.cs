namespace ValidationFlow.Messages;

public record DeleteRequested(string AppName, string EntityType, Guid EntityId);

