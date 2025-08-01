using MassTransit;
using MassTransit.Testing;
using ValidationFlow.Messages;
using Validation.Domain.Entities;
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
            var item = new Item(5);
            await harness.InputQueueSendEndpoint.Send(new ValidationFlow.Messages.SaveRequested<Item>("test", nameof(Item), item.Id, item));
            Assert.True(await harness.Consumed.Any<ValidationFlow.Messages.SaveRequested<Item>>());
            Assert.True(await consumerHarness.Consumed.Any<ValidationFlow.Messages.SaveRequested<Item>>());
            Assert.Single(repository.Audits);
        }
        finally
        {
            await harness.Stop();
        }
    }
}
