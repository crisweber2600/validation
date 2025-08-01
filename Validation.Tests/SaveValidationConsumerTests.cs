using MassTransit;
using MassTransit.Testing;
using Validation.Domain.Events;
using Validation.Domain.Validation;
using Validation.Domain.Providers;
using Validation.Infrastructure.Messaging;
using Validation.Domain.Entities;

namespace Validation.Tests;

public class SaveValidationConsumerTests
{
    private class MockEntityIdProvider : IEntityIdProvider
    {
        public Guid GetEntityId<T>(T entity) => Guid.NewGuid();
        public bool CanHandle<T>() => true;
    }

    private class MockApplicationNameProvider : IApplicationNameProvider
    {
        public string GetApplicationName() => "TestApp";
        public string GetApplicationName(string? context) => "TestApp";
    }

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
        var entityIdProvider = new MockEntityIdProvider();
        var applicationNameProvider = new MockApplicationNameProvider();
        var consumer = new SaveValidationConsumer<Item>(new TestPlanProvider(), repository, new SummarisationValidator(), entityIdProvider, applicationNameProvider);

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