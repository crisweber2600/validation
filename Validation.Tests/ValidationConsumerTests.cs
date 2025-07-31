using MassTransit;
using MassTransit.Testing;
using Validation.Domain.Events;
using Validation.Domain.Validation;
using Validation.Infrastructure.Messaging;

namespace Validation.Tests;

public class ValidationConsumerTests
{
    private class StubPlanProvider : IValidationPlanProvider
    {
        private readonly IEnumerable<IValidationRule> _rules;
        public StubPlanProvider(IEnumerable<IValidationRule> rules) { _rules = rules; }
        public IEnumerable<IValidationRule> GetRules<T>() => _rules;
    }

    [Fact]
    public async Task SaveValidationConsumer_publishes_SaveValidated_event()
    {
        var repository = new InMemorySaveAuditRepository();
        var provider = new StubPlanProvider(new[] { new RawDifferenceRule(1000) });
        var validator = new SummarisationValidator();
        var consumer = new SaveValidationConsumer<object>(provider, repository, validator);

        var harness = new InMemoryTestHarness();
        var consumerHarness = harness.Consumer(() => consumer);
        await harness.Start();
        try
        {
            await harness.InputQueueSendEndpoint.Send(new SaveRequested(Guid.NewGuid()));
            Assert.True(await consumerHarness.Consumed.Any<SaveRequested>());
            Assert.True(await harness.Published.Any<SaveValidated<object>>());
        }
        finally
        {
            await harness.Stop();
        }
    }
}
