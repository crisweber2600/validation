using MassTransit.Testing;
using Validation.Domain.Events;
using Validation.Infrastructure.Messaging;
using Validation.Domain.Entities;
using Validation.Domain.Repositories;

namespace Validation.Tests;

public class DeleteCommitConsumerTests
{
    private class TestRepository : IEntityRepository<Item>
    {
        public Task SaveAsync(Item entity, string? app = null, CancellationToken ct = default) => Task.CompletedTask;
        public Task DeleteAsync(Guid id, string? app = null, CancellationToken ct = default) => Task.CompletedTask;
    }

    [Fact]
    public async Task Publish_DeleteCommitted_on_success()
    {
        var repo = new TestRepository();
        var consumer = new DeleteCommitConsumer<Item>(repo);
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
