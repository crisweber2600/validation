using System;
namespace Validation.Domain.Events;

[Obsolete("Use ValidationFlow.Messages.DeleteRequested<T> instead")]
public record DeleteRequested(Guid Id);
