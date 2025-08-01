namespace Validation.Domain.Events;

[System.Obsolete("Use ValidationFlow.Messages.SaveValidated<T> instead")]
public record SaveValidated(Guid Id, bool IsValid, decimal Metric);