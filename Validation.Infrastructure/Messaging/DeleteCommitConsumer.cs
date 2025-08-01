using System;
using System.Threading.Tasks;
using MassTransit;
using Validation.Domain.Events;
using Validation.Infrastructure.Repositories;

namespace Validation.Infrastructure.Messaging;

public class DeleteCommitConsumer<T> : IConsumer<DeleteValidated>
{
    private readonly ISaveAuditRepository _repository;

    public DeleteCommitConsumer(ISaveAuditRepository repository)
    {
        _repository = repository;
    }

    public async Task Consume(ConsumeContext<DeleteValidated> context)
    {
        try
        {
            await _repository.DeleteAsync(context.Message.AuditId, context.CancellationToken);
        }
        catch (Exception ex)
        {
            // In this simplified example we just log the exception via MassTransit
            await context.Publish(new DeleteCommitFault(context.Message.EntityId, context.Message.AuditId, ex.Message, typeof(T).Name));
        }
    }
}
