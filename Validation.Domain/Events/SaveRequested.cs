namespace Validation.Domain.Events;

[System.Obsolete("Use ValidationFlow.Messages.SaveRequested<T> instead")]
public record SaveRequested(Guid Id);
