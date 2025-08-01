using MassTransit;
using MassTransit.Testing;
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
        var consumer = new SaveRequestedConsumer(repository, rule);

        var harness = new InMemoryTestHarness();
        var consumerHarness = harness.Consumer(() => consumer);
        await harness.Start();
        try
        {
            await harness.InputQueueSendEndpoint.Send(new SaveRequested<object>("test", "Item", Guid.NewGuid(), new object()));
            Assert.True(await harness.Consumed.Any<SaveRequested<object>>());
            Assert.True(await consumerHarness.Consumed.Any<SaveRequested<object>>());
            Assert.Single(repository.Audits);
        }
        finally
        {
            await harness.Stop();
        }
    }
}
