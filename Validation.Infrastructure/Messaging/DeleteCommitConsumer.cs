using System;
using MassTransit;
using ValidationFlow.Messages;
using Validation.Infrastructure.Repositories;

namespace Validation.Infrastructure.Messaging;

public class DeleteCommitConsumer<T> : IConsumer<DeleteCommitRequested<T>>
{
    private readonly ISaveAuditRepository _repository;

    public DeleteCommitConsumer(ISaveAuditRepository repository)
    {
        _repository = repository;
    }

    public async Task Consume(ConsumeContext<DeleteCommitRequested<T>> context)
    {
        try
        {
            await _repository.DeleteAsync(context.Message.ValidationId, context.CancellationToken);
            await context.Publish(new DeleteCommitCompleted<T>(string.Empty, typeof(T).Name, context.Message.EntityId, context.Message.ValidationId));
        }
        catch (Exception ex)
        {
            await context.Publish(new DeleteCommitFault<T>(string.Empty, typeof(T).Name, context.Message.EntityId, ex.Message));
        }
    }
}
