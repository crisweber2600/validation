using MassTransit;
using MassTransit.Testing;
using Validation.Domain.Events;
using Validation.Infrastructure.Messaging;
using Validation.Infrastructure.Repositories;
using Validation.Infrastructure;
using Validation.Domain.Entities;

namespace Validation.Tests;

public class DeleteCommitConsumerTests
{
    private class FailingRepository : ISaveAuditRepository
    {
        public Task AddAsync(SaveAudit entity, CancellationToken ct = default) => Task.CompletedTask;
        public Task DeleteAsync(Guid id, CancellationToken ct = default) => throw new Exception("fail");
        public Task<SaveAudit?> GetAsync(Guid id, CancellationToken ct = default) => Task.FromResult<SaveAudit?>(new SaveAudit { Id = id, EntityId = id });
        public Task UpdateAsync(SaveAudit entity, CancellationToken ct = default) => Task.CompletedTask;
        public Task<SaveAudit?> GetLastAsync(Guid entityId, CancellationToken ct = default) => Task.FromResult<SaveAudit?>(null);
    }

    [Fact]
    public async Task Publish_DeleteCommitFault_on_error()
    {
        var repo = new FailingRepository();
        var consumer = new DeleteCommitConsumer<Item>(repo);

        var harness = new InMemoryTestHarness();
        harness.Consumer(() => consumer);

        await harness.Start();
        try
        {
            await harness.InputQueueSendEndpoint.Send(new DeleteValidated<Item>(Guid.NewGuid()));

            Assert.True(await harness.Published.Any<DeleteCommitFault<Item>>());
        }
        finally
        {
            await harness.Stop();
        }
    }
}
