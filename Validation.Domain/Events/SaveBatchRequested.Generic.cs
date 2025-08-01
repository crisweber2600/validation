namespace Validation.Domain.Events;

public record SaveBatchRequested<T>(Guid BatchId, IEnumerable<T> Items);
