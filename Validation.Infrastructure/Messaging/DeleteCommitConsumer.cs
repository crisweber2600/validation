using MassTransit;
using Validation.Domain.Events;
using Validation.Domain.Repositories;

namespace Validation.Infrastructure.Messaging;

public class DeleteCommitConsumer<T> : IConsumer<DeleteValidated>
{
    private readonly IEntityRepository<T> _repository;

    public DeleteCommitConsumer(IEntityRepository<T> repository)
    {
        _repository = repository;
    }

    public async Task Consume(ConsumeContext<DeleteValidated> context)
    {
        await _repository.DeleteAsync(context.Message.Id, null, context.CancellationToken);
        await context.Publish(new DeleteCommitted(context.Message.Id), context.CancellationToken);
    }
}
