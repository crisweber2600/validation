using MassTransit;
using MassTransit.Testing;
using Validation.Domain.Events;
using Validation.Domain.Validation;
using Validation.Infrastructure.Messaging;
using Validation.Domain.Entities;

namespace Validation.Tests;

public class SaveValidationConsumerTests
{
    private class FailingPlanProvider : IValidationPlanProvider
    {
        public IEnumerable<IValidationRule> GetRules<T>() => new[] { new RawDifferenceRule(1) };
    }

    [Fact]
    public async Task Publish_SaveValidated_false_when_rule_fails()
    {
        var repository = new InMemorySaveAuditRepository();
        var consumer = new SaveValidationConsumer<Item>(new FailingPlanProvider(), repository, new SummarisationValidator());

        var harness = new InMemoryTestHarness();
        harness.Consumer(() => consumer);

        await harness.Start();
        try
        {
            await harness.InputQueueSendEndpoint.Send(new SaveRequested(Guid.NewGuid(), 5m));

            Assert.True(await harness.Published.Any<SaveValidated>());
            var message = (await harness.Published.SelectAsync<SaveValidated>().First()).Context.Message;
            Assert.False(message.Validated);
        }
        finally
        {
            await harness.Stop();
        }
    }
}