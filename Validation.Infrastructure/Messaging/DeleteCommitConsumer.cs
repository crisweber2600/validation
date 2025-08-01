using MassTransit;
using Validation.Domain.Events;

namespace Validation.Infrastructure.Messaging;

public class DeleteCommitConsumer<T> : IConsumer<DeleteRequested>
{
    public Task Consume(ConsumeContext<DeleteRequested> context)
    {
        // In a full implementation this would perform the delete operation
        return Task.CompletedTask;
    }
}
