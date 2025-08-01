using MassTransit;
using Validation.Domain.Events;
namespace Validation.Infrastructure.Messaging;

public class DeleteCommitConsumer<T> : IConsumer<DeleteValidated<T>>
{
    public DeleteCommitConsumer()
    {
    }

    public async Task Consume(ConsumeContext<DeleteValidated<T>> context)
    {
        try
        {
            // commit logic would delete from repository in real implementation
            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            await context.Publish(new DeleteCommitFault<T>(context.Message.EntityId, ex.Message));
        }
    }
}
