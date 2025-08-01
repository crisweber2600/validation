using MassTransit;
using MassTransit.Testing;
using Validation.Domain.Events;
using Validation.Domain.Validation;
using Validation.Domain;
using Validation.Infrastructure.Messaging;
using Validation.Domain.Entities;
using Validation.Infrastructure;

namespace Validation.Tests;

public class SaveValidationConsumerTests
{
    private class TestPlanProvider : IValidationPlanProvider
    {
        public IEnumerable<IValidationRule> GetRules<T>() => new[] { new RawDifferenceRule(100) };
        public ValidationPlan GetPlan(Type t) => new ValidationPlan(
            o => ((Item)o).Metric,
            ThresholdType.RawDifference,
            100m);
        public void AddPlan<T>(ValidationPlan plan) { }
    }

    private class TestIdProvider : IEntityIdProvider
    {
        public Guid GetId<T>(T entity) => ((Item)(object)entity).Id;
    }

    private class TestAppProvider : IApplicationNameProvider
    {
        public string ApplicationName => "TestApp";
    }

    [Fact]
    public async Task Publish_SaveValidated_after_processing()
    {
        var repository = new InMemorySaveAuditRepository();
        var consumer = new SaveValidationConsumer<Item>(
            new TestPlanProvider(),
            repository,
            new ManualValidatorService(),
            new TestIdProvider(),
            new TestAppProvider());

        var harness = new InMemoryTestHarness();
        harness.Consumer(() => consumer);

        await harness.Start();
        try
        {
            await harness.InputQueueSendEndpoint.Send(new SaveRequested<Item>(new Item(10)));

            Assert.True(await harness.Published.Any<SaveValidated<Item>>());
        }
        finally
        {
            await harness.Stop();
        }
    }
}