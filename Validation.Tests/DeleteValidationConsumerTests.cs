using MassTransit;
using MassTransit.Testing;
using Validation.Domain.Events;
using Validation.Domain.Validation;
using Validation.Infrastructure.Messaging;

namespace Validation.Tests;

public class DeleteValidationConsumerTests
{
    private class TestPlanProvider : IValidationPlanProvider
    {
        public IEnumerable<IValidationRule> GetRules<T>() => new[] { new RawDifferenceRule(100) };
    }

    [Fact]
    public async Task Publish_DeleteValidated_after_processing()
    {
        var consumer = new DeleteValidationConsumer<string>(new TestPlanProvider(), new SummarisationValidator());

        var harness = new InMemoryTestHarness();
        harness.Consumer(() => consumer);

        await harness.Start();
        try
        {
            await harness.InputQueueSendEndpoint.Send(new DeleteRequested(Guid.NewGuid()));

            Assert.True(await harness.Published.Any<DeleteValidated>());
        }
        finally
        {
            await harness.Stop();
        }
    }
}
