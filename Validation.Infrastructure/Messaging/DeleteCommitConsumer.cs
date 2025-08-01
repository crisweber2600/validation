using MassTransit;
using Validation.Domain.Events;
using Validation.Infrastructure.Repositories;
using Validation.Infrastructure;

namespace Validation.Infrastructure.Messaging;

public class DeleteCommitConsumer<T> : IConsumer<DeleteValidated<T>> where T : class
{
    private readonly UnitOfWork _uow;

    public DeleteCommitConsumer(UnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task Consume(ConsumeContext<DeleteValidated<T>> context)
    {
        await _uow.Repository<T>().DeleteAsync(context.Message.EntityId, context.CancellationToken);
        await _uow.SaveChangesWithPlanAsync<T>(context.CancellationToken);
    }
}
