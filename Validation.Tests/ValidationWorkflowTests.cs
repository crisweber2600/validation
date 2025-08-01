using MassTransit;
using MassTransit.Testing;
using Validation.Domain.Entities;
using ValidationFlow.Messages;
using Validation.Domain.Validation;
using Validation.Infrastructure.Messaging;

namespace Validation.Tests;

public class ValidationWorkflowTests
{
    [Fact]
    public async Task SaveRequested_event_creates_audit_record()
    {
        var repository = new InMemorySaveAuditRepository();
        var rule = new RawDifferenceRule(100); // always valid
        var consumer = new SaveRequestedConsumer<Item>(repository, rule);

        var harness = new InMemoryTestHarness();
        var consumerHarness = harness.Consumer(() => consumer);
        await harness.Start();
        try
        {
            var msg = new SaveRequested<Item>("TestApp", nameof(Item), Guid.NewGuid(), new Item(0));
            await harness.InputQueueSendEndpoint.Send(msg);
            Assert.True(await harness.Consumed.Any<SaveRequested<Item>>());
            Assert.True(await consumerHarness.Consumed.Any<SaveRequested<Item>>());
            Assert.Single(repository.Audits);
        }
        finally
        {
            await harness.Stop();
        }
    }
}
