namespace ValidationFlow.Messages;

public record DeleteValidated(string AppName, string EntityType, Guid EntityId);

