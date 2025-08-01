using System;
using MassTransit;
using Validation.Domain.Events;
using Validation.Infrastructure.Repositories;

namespace Validation.Infrastructure.Messaging;

/// <summary>
/// Finalizes delete requests by removing the associated audit record. If the
/// repository operation fails, a <see cref="DeleteCommitFault{T}"/> event is
/// published so that the delete can be retried from the error queue.
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
            await _repository.DeleteAsync(context.Message.AuditId, context.CancellationToken);
        }
        catch (Exception ex)
        {
            await context.Publish(new DeleteCommitFault<T>(context.Message.EntityId, context.Message.AuditId, ex.Message));
        }
    }
}
