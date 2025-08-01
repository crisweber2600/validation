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
            var audit = await _repository.GetAsync(context.Message.AuditId, context.CancellationToken);
            if (audit != null)
            {
                await _repository.DeleteAsync(audit.Id, context.CancellationToken);
            }
        }
        catch (Exception ex)
        {
            await context.Publish(new DeleteCommitFault<T>(context.Message.EntityId, context.Message.AuditId, ex.Message));
        }
    }
}
