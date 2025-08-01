namespace Validation.Domain.Events;

[System.Obsolete("Use ValidationFlow.Messages.DeleteRequested<T> instead")]
public record DeleteRequested(Guid Id);
