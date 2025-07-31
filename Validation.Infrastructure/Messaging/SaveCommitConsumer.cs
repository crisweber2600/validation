using MassTransit;
using Validation.Domain.Events;
using Validation.Infrastructure.Repositories;
using Validation.Infrastructure;

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
            var audit = new SaveAudit
            {
                Id = Guid.NewGuid(),
                EntityId = context.Message.Id,
                IsValid = context.Message.IsValid,
                Metric = 0
            };
            await _repository.AddAsync(audit, context.CancellationToken);
        }
        catch (Exception ex)
        {
            await context.Publish(new SaveCommitFault<T>(context.Message.Id, ex.Message));
        }
    }
}
