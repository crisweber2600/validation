namespace Validation.Domain.Events;

[System.Obsolete("Use ValidationFlow.Messages.SaveRequested<T> instead")]
public record SaveRequested<T>(T Entity, string? App = null);