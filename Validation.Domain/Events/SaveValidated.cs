namespace Validation.Domain.Events;

public record SaveValidated(Guid Id, bool IsValid, decimal Metric);
