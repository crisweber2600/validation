using System;

namespace Validation.Domain.Events;

[Obsolete("Use ValidationFlow.Messages.DeleteRequested message instead")]
public record DeleteRequested(Guid Id);
