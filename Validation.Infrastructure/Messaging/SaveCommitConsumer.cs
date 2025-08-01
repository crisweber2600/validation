using MassTransit;
using ValidationFlow.Messages;
using Validation.Infrastructure.Repositories;

namespace Validation.Infrastructure.Messaging;

public class SaveCommitConsumer<T> : IConsumer<SaveCommitRequested<T>>
{
    private readonly ISaveAuditRepository _repository;

    public SaveCommitConsumer(ISaveAuditRepository repository)
    {
        _repository = repository;
    }

    public async Task Consume(ConsumeContext<SaveCommitRequested<T>> context)
    {
        try
        {
            var audit = await _repository.GetAsync(context.Message.ValidationId, context.CancellationToken);
            if (audit != null)
            {
                await _repository.UpdateAsync(audit, context.CancellationToken);
            }
            await context.Publish(new SaveCommitCompleted<T>(context.Message.AppName, context.Message.EntityType, context.Message.EntityId, context.Message.Payload, context.Message.ValidationId));
        }
        catch (Exception ex)
        {
            await context.Publish(new SaveCommitFault<T>(context.Message.AppName, context.Message.EntityType, context.Message.EntityId, context.Message.Payload, ex.Message));
        }
    }
}