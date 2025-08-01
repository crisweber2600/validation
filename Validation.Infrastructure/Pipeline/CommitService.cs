using MassTransit;
using Validation.Domain.Events;
using Validation.Infrastructure.Repositories;

namespace Validation.Infrastructure.Pipeline;

/// <summary>
/// Persists a validation audit and publishes a <see cref="SaveValidated{T}"/> event.
/// </summary>
public class CommitService
{
    private readonly ISaveAuditRepository _repository;
    private readonly IPublishEndpoint _publish;

    public CommitService(ISaveAuditRepository repository, IPublishEndpoint publish)
    {
        _repository = repository;
        _publish = publish;
    }

    public async Task CommitAsync<T>(Guid entityId, decimal metric, CancellationToken ct = default)
    {
        var audit = new SaveAudit
        {
            Id = Guid.NewGuid(),
            EntityId = entityId,
            IsValid = true,
            Metric = metric
        };
        await _repository.AddAsync(audit, ct);
        await _publish.Publish(new SaveValidated<T>(entityId, audit.Id), ct);
    }
}
