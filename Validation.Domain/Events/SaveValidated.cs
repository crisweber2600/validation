using System;

namespace Validation.Domain.Events;

[Obsolete("Use ValidationFlow.Messages.SaveValidated message instead")]
public record SaveValidated(Guid Id, bool IsValid, decimal Metric);