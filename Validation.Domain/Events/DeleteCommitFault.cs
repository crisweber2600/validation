namespace Validation.Domain.Events;

public record DeleteCommitFault<T>(Guid EntityId, string Error);
