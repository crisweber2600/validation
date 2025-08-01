using MassTransit;
using MassTransit.Testing;
using Validation.Domain.Entities;
using Validation.Domain.Events;
using Validation.Domain.Validation;
using ValidationFlow.Messages;
using Validation.Infrastructure.Messaging;
using Validation.Infrastructure.Repositories;
using Validation.Infrastructure;
using Validation.Domain;

namespace Validation.Tests;

public class SaveBatchValidationConsumerTests
{
    private class TestPlanProvider : IValidationPlanProvider
    {
        private readonly decimal _threshold;
        public TestPlanProvider(decimal threshold) => _threshold = threshold;
        public IEnumerable<IValidationRule> GetRules<T>() => Array.Empty<IValidationRule>();
        public ValidationPlan GetPlan(Type t) => new ValidationPlan(o => ((Item)o).Metric, ThresholdType.RawDifference, _threshold);
        public void AddPlan<T>(ValidationPlan plan) { }
    }

    private class IdProvider : IEntityIdProvider
    {
        public Guid GetId<T>(T entity) => entity is Item i ? i.Id : Guid.NewGuid();
    }

    private class AppProvider : IApplicationNameProvider
    {
        public string ApplicationName => "test-app";
    }

    [Fact]
    public async Task Publish_SaveValidated_after_processing()
    {
        var repo = new InMemorySaveAuditRepository();
        var manual = new ManualValidatorService();
        var consumer = new SaveBatchValidationConsumer<Item>(new TestPlanProvider(10m), repo, manual, new IdProvider(), new AppProvider());

        var harness = new InMemoryTestHarness();
        harness.Consumer(() => consumer);

        await harness.Start();
        try
        {
            var batch = new[] { new Item(1m), new Item(2m) };
            await harness.InputQueueSendEndpoint.Send(new SaveBatchRequested<Item>(Guid.NewGuid(), batch));

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
        manual.AddRule<Item>(i => i.Metric < 5);
        var consumer = new SaveBatchValidationConsumer<Item>(new TestPlanProvider(10m), repo, manual, new IdProvider(), new AppProvider());

        var harness = new InMemoryTestHarness();
        harness.Consumer(() => consumer);

        await harness.Start();
        try
        {
            var batch = new[] { new Item(10m) };
            await harness.InputQueueSendEndpoint.Send(new SaveBatchRequested<Item>(Guid.NewGuid(), batch));

            Assert.True(await harness.Published.Any<Validation.Domain.Events.ValidationOperationFailed>());
        }
        finally
        {
            await harness.Stop();
        }
    }

    [Fact]
    public async Task Publish_ValidationFailed_when_sequence_invalid()
    {
        var repo = new InMemorySaveAuditRepository();
        // existing audit with metric 1
        await repo.AddAsync(new SaveAudit { Id = Guid.NewGuid(), EntityId = Guid.NewGuid(), Metric = 1m, IsValid = true });

        var manual = new ManualValidatorService();
        var consumer = new SaveBatchValidationConsumer<Item>(new TestPlanProvider(0m), repo, manual, new IdProvider(), new AppProvider());

        var harness = new InMemoryTestHarness();
        harness.Consumer(() => consumer);

        await harness.Start();
        try
        {
            var batch = new[] { new Item(3m) };
            await harness.InputQueueSendEndpoint.Send(new SaveBatchRequested<Item>(Guid.NewGuid(), batch));

            Assert.True(await harness.Published.Any<Validation.Domain.Events.ValidationOperationFailed>());
        }
        finally
        {
            await harness.Stop();
        }
    }
}
