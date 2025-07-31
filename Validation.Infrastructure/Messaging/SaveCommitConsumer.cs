using MassTransit;
using Validation.Domain.Events;
using Validation.Infrastructure.Repositories;

namespace Validation.Infrastructure.Messaging;

public class SaveCommitConsumer<T> : IConsumer<SaveValidated<T>>
{
    private readonly ISaveAuditRepository _repository;

    public SaveCommitConsumer(ISaveAuditRepository repository)
    {
        _repository = repository;
    }

    public async Task Consume(ConsumeContext<SaveValidated<T>> context)
    {
        try
        {
            var audit = await _repository.GetLastForEntityAsync(context.Message.Id, context.CancellationToken);
            if (audit != null)
            {
                await _repository.UpdateAsync(audit, context.CancellationToken);
            }
        }
        catch
        {
            await context.Publish(new SaveCommitFault<T>(context.Message.Id));
        }
    }
}
