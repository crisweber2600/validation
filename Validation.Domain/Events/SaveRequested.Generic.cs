using System;
namespace Validation.Domain.Events;

[Obsolete("Use ValidationFlow.Messages.SaveRequested instead")]
public record SaveRequested<T>(T Entity, string? App = null);