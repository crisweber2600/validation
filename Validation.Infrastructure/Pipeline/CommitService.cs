using MassTransit;
using Validation.Domain.Events;
using Validation.Infrastructure.Repositories;

namespace Validation.Infrastructure.Pipeline;

public class CommitService
{
    private readonly ISaveAuditRepository _repository;
    private readonly IPublishEndpoint _publish;

    public CommitService(ISaveAuditRepository repository, IPublishEndpoint publish)
    {
        _repository = repository;
        _publish = publish;
    }

    public async Task CommitAsync<T>(decimal metric, bool isValid, CancellationToken ct)
    {
        var audit = new SaveAudit
        {
            Id = Guid.NewGuid(),
            EntityId = Guid.Empty,
            IsValid = isValid,
            Metric = metric
        };
        await _repository.AddAsync(audit, ct);
        await _publish.Publish(new SaveValidated<T>(audit.EntityId, audit.Id), ct);
    }
}
