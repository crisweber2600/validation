using MassTransit;
using MassTransit.Testing;
using Validation.Domain.Events;
using Validation.Domain.Validation;
using Validation.Infrastructure.Messaging;
using Validation.Domain.Entities;

namespace Validation.Tests;

public class SaveValidationConsumerTests
{
    private class TestPlanProvider : IValidationPlanProvider
    {
        public IEnumerable<IValidationRule> GetRules<T>() => new[] { new RawDifferenceRule(100) };
    }

    [Fact]
    public async Task Publish_SaveValidated_after_processing()
    {
        var repository = new InMemorySaveAuditRepository();
        var consumer = new SaveValidationConsumer<Item>(new TestPlanProvider(), repository, new SummarisationValidator());

        var harness = new InMemoryTestHarness();
        harness.Consumer(() => consumer);

        await harness.Start();
        try
        {
            await harness.InputQueueSendEndpoint.Send(new SaveRequested(Guid.NewGuid()));

            Assert.True(await harness.Published.Any<SaveValidated<Item>>());
        }
        finally
        {
            await harness.Stop();
        }
    }
}
