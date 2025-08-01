namespace Validation.Domain.Events;

public record SaveRequested<T>(Guid Id, T Entity);