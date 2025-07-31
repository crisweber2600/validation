using MassTransit;
using MassTransit.Testing;
using Validation.Domain.Events;
using Validation.Domain.Validation;
using Validation.Infrastructure.Messaging;
using Validation.Domain.Entities;

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
            await harness.InputQueueSendEndpoint.Send(new SaveRequested<Item>(Guid.NewGuid()));
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
