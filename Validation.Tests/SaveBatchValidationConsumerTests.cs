using MassTransit;
using MassTransit.Testing;
using Validation.Domain.Entities;
using Validation.Domain.Validation;
using Validation.Infrastructure.Messaging;
using Validation.Infrastructure.Repositories;
using ValidationFlow.Messages;
using Validation.Domain;
using Validation.Infrastructure;

namespace Validation.Tests;

public class SaveBatchValidationConsumerTests
{
    private class TestPlanProvider : IValidationPlanProvider
    {
        public IEnumerable<IValidationRule> GetRules<T>() => Array.Empty<IValidationRule>();
        public ValidationPlan GetPlan(Type t) => new ValidationPlan(o => ((Item)o).Metric, ThresholdType.RawDifference, 100m);
        public void AddPlan<T>(ValidationPlan plan) { }
    }

    private class DummyIdProvider : IEntityIdProvider
    {
        public Guid GetId(object entity) => ((Item)entity).Id;
    }

    private class DummyAppProvider : IApplicationNameProvider
    {
        public string ApplicationName => "TestApp";
    }

    [Fact]
    public async Task Publish_SaveValidated_after_processing_batch()
    {
        var repo = new InMemorySaveAuditRepository();
        var consumer = new SaveBatchValidationConsumer<Item>(
            new TestPlanProvider(),
            repo,
            new ManualValidatorService(),
            new DummyIdProvider(),
            new DummyAppProvider());

        var harness = new InMemoryTestHarness();
        harness.Consumer(() => consumer);

        await harness.Start();
        try
        {
            var items = new[] { new Item(10), new Item(20) };
            await harness.InputQueueSendEndpoint.Send(new SaveBatchRequested<Item>(Guid.NewGuid(), items));

            Assert.True(await harness.Published.Any<Validation.Domain.Events.SaveValidated<Item>>());
            var audit = Assert.Single(repo.Audits);
            Assert.Equal(2, audit.BatchSize);
        }
        finally
        {
            await harness.Stop();
        }
    }
}
