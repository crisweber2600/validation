using MassTransit;
using MassTransit.Testing;
using ValidationFlow.Messages;
using Validation.Domain.Validation;
using Validation.Infrastructure.Messaging;
using Validation.Domain.Entities;

namespace Validation.Tests;

public class SaveValidationConsumerTests
{
    private class TestPlanProvider : IValidationPlanProvider
    {
        public IEnumerable<IValidationRule> GetRules<T>() => new[] { new RawDifferenceRule(100) };
        public ValidationPlan GetPlan(Type t) => new ValidationPlan(new[] { new RawDifferenceRule(100) });
        public void AddPlan<T>(ValidationPlan plan) { }
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
            await harness.InputQueueSendEndpoint.Send(new SaveRequested<Item>("TestApp", nameof(Item), Guid.NewGuid(), new Item(0)));

            Assert.True(await harness.Published.Any<SaveValidated<Item>>());
        }
        finally
        {
            await harness.Stop();
        }
    }
}