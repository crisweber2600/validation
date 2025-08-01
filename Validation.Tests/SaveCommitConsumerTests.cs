using MassTransit;
using MassTransit.Testing;
using ValidationFlow.Messages;
using Validation.Infrastructure.Messaging;
using Validation.Infrastructure;
using System.Linq;
using Validation.Infrastructure.Repositories;
using Validation.Domain.Entities;

namespace Validation.Tests;

public class SaveCommitConsumerTests
{
    private class FailingRepository : ISaveAuditRepository
    {
        public Task AddAsync(SaveAudit entity, CancellationToken ct = default) => Task.CompletedTask;
        public Task DeleteAsync(Guid id, CancellationToken ct = default) => Task.CompletedTask;
        public Task<SaveAudit?> GetAsync(Guid id, CancellationToken ct = default) => Task.FromResult<SaveAudit?>(new SaveAudit { Id = id, EntityId = id });
        public Task UpdateAsync(SaveAudit entity, CancellationToken ct = default) => throw new Exception("fail");
        public Task<SaveAudit?> GetLastAsync(Guid entityId, CancellationToken ct = default) => Task.FromResult<SaveAudit?>(new SaveAudit { Id = entityId, EntityId = entityId });
    }

    [Fact]
    public async Task Publish_SaveCommitFault_on_error()
    {
        var repo = new FailingRepository();
        var consumer = new SaveCommitConsumer<Item>(repo);

        var harness = new InMemoryTestHarness();
        harness.Consumer(() => consumer);

        await harness.Start();
        try
        {
            await harness.InputQueueSendEndpoint.Send(new SaveValidated<Item>("TestApp", nameof(Item), Guid.NewGuid(), new Item(0), true));

            var published = harness.Published.Select<SaveCommitFault<Item>>().First().Context.Message;
            Assert.Equal("TestApp", published.AppName);
        }
        finally
        {
            await harness.Stop();
        }
    }
}