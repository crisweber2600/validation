using System;
namespace Validation.Domain.Events;

[Obsolete("Use ValidationFlow.Messages.DeleteRequested instead")]
public record DeleteRequested(Guid Id);
