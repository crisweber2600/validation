using MassTransit;
using Validation.Domain.Events;
using Validation.Infrastructure.Repositories;

namespace Validation.Infrastructure.Pipeline;

/// <summary>
/// Persists audits and notifies interested consumers when validation succeeds.
/// </summary>
public class CommitService
{
    private readonly ISaveAuditRepository _repository;
    private readonly IPublishEndpoint _publisher;

    public CommitService(ISaveAuditRepository repository, IPublishEndpoint publisher)
    {
        _repository = repository;
        _publisher = publisher;
    }

    /// <summary>
    /// Store the audit and publish a <see cref="SaveValidated{T}"/> event.
    /// </summary>
    public async Task CommitAsync<T>(Guid entityId, decimal metric, bool valid, CancellationToken ct = default)
    {
        var audit = new SaveAudit
        {
            Id = Guid.NewGuid(),
            EntityId = entityId,
            IsValid = valid,
            Metric = metric
        };
        await _repository.AddAsync(audit, ct);
        await _publisher.Publish(new SaveValidated<T>(entityId, audit.Id), ct);
    }
}
