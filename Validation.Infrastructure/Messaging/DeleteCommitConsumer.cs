using MassTransit;
using Validation.Domain.Events;

namespace Validation.Infrastructure.Messaging;

public class DeleteCommitConsumer : IConsumer<DeleteValidated>
{
    public async Task Consume(ConsumeContext<DeleteValidated> context)
    {
        await context.Publish(new DeleteCommitted(context.Message.Id));
    }
}
