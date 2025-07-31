namespace Validation.Domain.Events;

public record SaveRequested<T>(Guid Id, string? App = null);
