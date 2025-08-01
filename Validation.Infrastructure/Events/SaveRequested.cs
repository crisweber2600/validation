namespace Validation.Infrastructure.Events;

public record SaveRequested<T>(Guid Id, T Entity);
