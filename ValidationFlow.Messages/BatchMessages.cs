using System;
using System.Collections.Generic;

namespace ValidationFlow.Messages.Batch;

/// <summary>
/// Request to persist a batch of entities.
/// </summary>
[Serializable]
public sealed record SaveBatchRequested<T>(Guid BatchId, IEnumerable<T> Items);

/// <summary>
/// Request to persist a single entity.
/// </summary>
[Serializable]
public sealed record SaveRequested<T>(Guid RequestId, T Item);
