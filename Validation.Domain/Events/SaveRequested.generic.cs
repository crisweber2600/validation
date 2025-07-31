namespace Validation.Domain.Events;

public record SaveRequested<T>(T Entity, string? App = null);
