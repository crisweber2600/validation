namespace ValidationFlow.Messages;
public record SaveBatchRequested<T>(Guid CorrelationId, IEnumerable<T> Entities);
