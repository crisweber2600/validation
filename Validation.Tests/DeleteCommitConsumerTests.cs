using MassTransit;
using MassTransit.Testing;
using Validation.Domain.Events;
using Validation.Infrastructure.Messaging;

namespace Validation.Tests;

public class DeleteCommitConsumerTests
{
    [Fact]
    public async Task Publish_DeleteCommitted_after_processing()
    {
        var consumer = new DeleteCommitConsumer();
        var harness = new InMemoryTestHarness();
        harness.Consumer(() => consumer);

        await harness.Start();
        try
        {
            await harness.InputQueueSendEndpoint.Send(new DeleteValidated(Guid.NewGuid(), true));

            Assert.True(await harness.Published.Any<DeleteCommitted>());
        }
        finally
        {
            await harness.Stop();
        }
    }
}
