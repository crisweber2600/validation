using MassTransit;
using Validation.Domain.Events;
using Validation.Infrastructure.Repositories;

namespace Validation.Infrastructure.Messaging;

/// <summary>
/// Handles commit operations for delete requests. Removes audit records and
/// publishes <see cref="DeleteCommitFault{T}"/> when a repository error occurs.
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
            var audit = await _repository.GetLastAsync(context.Message.EntityId, context.CancellationToken);
            if (audit != null)
            {
                await _repository.DeleteAsync(audit.Id, context.CancellationToken);
            }
        }
        catch (Exception ex)
        {
            await context.Publish(new DeleteCommitFault<T>(context.Message.EntityId, ex.Message));
        }
    }
}
