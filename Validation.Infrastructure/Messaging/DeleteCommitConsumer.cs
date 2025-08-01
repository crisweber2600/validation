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
            await context.Publish(new DeleteCommitFault<T>(context.Message.EntityId, context.Message.AuditId, ex.Message));
        }
    }
}
