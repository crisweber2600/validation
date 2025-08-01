using MassTransit;
using Validation.Domain.Events;
using Validation.Infrastructure.Repositories;

namespace Validation.Infrastructure.Messaging;

/// <summary>
/// Consumes <see cref="DeleteValidated{T}"/> events and removes the associated audit record.
/// Publishes <see cref="DeleteCommitFault{T}"/> if the repository throws.
/// </summary>
public class DeleteCommitConsumer<T> : IConsumer<DeleteValidated<T>>
{
    private readonly ISaveAuditRepository _repository;

    public DeleteCommitConsumer(ISaveAuditRepository repository)
    {
        _repository = repository;
    }

    public async Task Consume(ConsumeContext<DeleteValidated<T>> context)
    {
        try
        {
            await _repository.DeleteAsync(context.Message.EntityId, context.CancellationToken);
        }
        catch (Exception ex)
        {
            await context.Publish(new DeleteCommitFault<T>(context.Message.EntityId, ex.Message));
        }
    }
}
