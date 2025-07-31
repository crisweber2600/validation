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
            var audit = await _repository.GetAsync(context.Message.Id, context.CancellationToken);
            if (audit == null)
            {
                audit = new SaveAudit
                {
                    Id = Guid.NewGuid(),
                    EntityId = context.Message.Id,
                    Metric = 0,
                    IsValid = context.Message.IsValid
                };
                await _repository.AddAsync(audit, context.CancellationToken);
            }
            else
            {
                audit.IsValid = context.Message.IsValid;
                await _repository.UpdateAsync(audit, context.CancellationToken);
            }
        }
        catch (Exception ex)
        {
            await context.Publish(new SaveCommitFault<T>(context.Message.Id, ex.Message), context.CancellationToken);
        }
    }
}
