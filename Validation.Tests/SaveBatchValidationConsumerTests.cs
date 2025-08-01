using MassTransit;
using MassTransit.Testing;
using Validation.Domain.Entities;
using Validation.Domain.Events;
using Validation.Domain.Validation;
using Validation.Infrastructure.Messaging;
using Validation.Infrastructure.Repositories;
using Validation.Infrastructure;

using ValidationFlow.Messages;
namespace Validation.Tests;

public class SaveBatchValidationConsumerTests
{
    private class TestPlanProvider : IValidationPlanProvider
    {
        public IEnumerable<IValidationRule> GetRules<T>() => Array.Empty<IValidationRule>();
        public ValidationPlan GetPlan(Type t) => new ValidationPlan(o => ((Item)o).Metric, ThresholdType.RawDifference, 100m);
        public void AddPlan<T>(ValidationPlan plan) { }
    }

    private class IdProvider : IEntityIdProvider
    {
        public Guid GetId<T>(T entity) => ((Item)(object)entity).Id;
    }

    private class AppProvider : IApplicationNameProvider
    {
        public string ApplicationName => "TestApp";
    }

    [Fact]
    public async Task Publish_SaveValidated_after_valid_batch()
    {
        var repo = new InMemorySaveAuditRepository();
        var consumer = new SaveBatchValidationConsumer<Item>(new TestPlanProvider(), repo, new ManualValidatorService(), new IdProvider(), new AppProvider());

        var harness = new InMemoryTestHarness();
        harness.Consumer(() => consumer);

        await harness.Start();
        try
        {
            var items = new[] { new Item(1), new Item(2) };
            await harness.InputQueueSendEndpoint.Send(new SaveBatchRequested<Item>(Guid.NewGuid(), items));

            Assert.True(await harness.Published.Any<Validation.Domain.Events.SaveValidated<Item>>());
        }
        finally
        {
            await harness.Stop();
        }
    }

    [Fact]
    public async Task Publish_ValidationFailed_when_manual_rule_fails()
    {
        var repo = new InMemorySaveAuditRepository();
        var manual = new ManualValidatorService();
        manual.AddRule<Item>(i => i.Metric > 0);
        var consumer = new SaveBatchValidationConsumer<Item>(new TestPlanProvider(), repo, manual, new IdProvider(), new AppProvider());

        var harness = new InMemoryTestHarness();
        harness.Consumer(() => consumer);

        await harness.Start();
        try
        {
            var items = new[] { new Item(-1) };
            await harness.InputQueueSendEndpoint.Send(new SaveBatchRequested<Item>(Guid.NewGuid(), items));

            Assert.True(await harness.Published.Any<ValidationOperationFailed>());
        }
        finally
        {
            await harness.Stop();
        }
    }
}
