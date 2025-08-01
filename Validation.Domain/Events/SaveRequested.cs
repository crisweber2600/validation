using System;

namespace Validation.Domain.Events;

[Obsolete("Use ValidationFlow.Messages.SaveRequested message instead")]
public record SaveRequested(Guid Id);
