using MassTransit;
using MassTransit.Testing;
using Validation.Domain.Events;
using Validation.Domain.Validation;
using System.Collections.Generic;
using Validation.Infrastructure.Messaging;
using Validation.Domain.Entities;

namespace Validation.Tests;

public class SaveValidationConsumerTests
{
    private class TestPlanProvider : IValidationPlanProvider
    {
        private readonly Dictionary<Type, ValidationPlan> _plans = new();

        public ValidationPlan GetPlan(Type t) => _plans[t];

        public void AddPlan<T>(ValidationPlan plan)
        {
            _plans[typeof(T)] = plan;
        }
    }

    [Fact]
    public async Task Publish_SaveValidated_after_processing()
    {
        var repository = new InMemorySaveAuditRepository();
        var provider = new TestPlanProvider();
        provider.AddPlan<Item>(new ValidationPlan(new[] { new RawDifferenceRule(100) }));
        var consumer = new SaveValidationConsumer<Item>(provider, repository, new SummarisationValidator());

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