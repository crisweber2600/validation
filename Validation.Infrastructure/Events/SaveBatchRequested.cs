namespace Validation.Infrastructure.Events;

public record SaveBatchRequested<T>(Guid Id, IEnumerable<T> Items);
