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
        var consumer = new SaveRequestedConsumer<Validation.Domain.Entities.Item>(repository, rule);

        var harness = new InMemoryTestHarness();
        var consumerHarness = harness.Consumer(() => consumer);
        await harness.Start();
        try
        {
            await harness.InputQueueSendEndpoint.Send(new SaveRequested<Validation.Domain.Entities.Item>("app","Item", Guid.NewGuid(), new Validation.Domain.Entities.Item(0)));
            Assert.True(await harness.Consumed.Any<SaveRequested<Validation.Domain.Entities.Item>>());
            Assert.True(await consumerHarness.Consumed.Any<SaveRequested<Validation.Domain.Entities.Item>>());
            Assert.Single(repository.Audits);
        }
        finally
        {
            await harness.Stop();
        }
    }
}
