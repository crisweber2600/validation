using System;
namespace Validation.Domain.Events;

[Obsolete("Use ValidationFlow.Messages.SaveRequested<T> instead")]
public record SaveRequested(Guid Id);
