using System;
using System.Threading;
using System.Threading.Tasks;
using Validation.Infrastructure.Repositories;
using Validation.Domain.Events;

namespace Validation.Infrastructure.Pipeline;

/// <summary>
/// Persists successful metrics and publishes an event.
/// </summary>
public class CommitService
{
    private readonly ISaveAuditRepository _repository;
    private readonly IEventPublisher _publisher;

    public CommitService(ISaveAuditRepository repository, IEventPublisher publisher)
    {
        _repository = repository;
        _publisher = publisher;
    }

    /// <summary>
    /// Persist the summary and notify observers.
    /// </summary>
    public virtual async Task CommitAsync(decimal summary, bool valid, CancellationToken ct)
    {
        var audit = new SaveAudit
        {
            Id = Guid.NewGuid(),
            EntityId = Guid.Empty,
            Metric = summary,
            IsValid = valid
        };
        await _repository.AddAsync(audit, ct);
        await _publisher.PublishAsync(new SaveValidated<decimal>(audit.EntityId, audit.Id), ct);
    }
}
