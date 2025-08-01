using MassTransit;
using MassTransit.Testing;
using Validation.Domain.Events;
using Validation.Domain;
using Validation.Domain.Validation;
using Validation.Infrastructure.Messaging;
using Validation.Domain.Entities;
using Validation.Infrastructure;

namespace Validation.Tests;

public class SaveValidationConsumerTests
{
    private class TestPlanProvider : IValidationPlanProvider
    {
        public IEnumerable<IValidationRule> GetRules<T>() => new[] { new RawDifferenceRule(100) };
        public ValidationPlan GetPlan(Type t) => new ValidationPlan(new[] { new RawDifferenceRule(100) });
        public ValidationPlan GetPlanFor<T>() => new ValidationPlan(obj => 10m, ThresholdType.RawDifference, 100m);
        public void AddPlan<T>(ValidationPlan plan) { }
    }

    private class IdProvider : IEntityIdProvider
    {
        public Guid GetId<T>(T entity)
        {
            return (Guid)entity!.GetType().GetProperty("Id")!.GetValue(entity)!;
        }
    }

    private class AppNameProvider : IApplicationNameProvider
    {
        public string ApplicationName => "Test";
    }

    [Fact]
    public async Task Publish_SaveValidated_after_processing()
    {
        var repository = new InMemorySaveAuditRepository();
        var manual = new ManualValidatorService();
        var idProvider = new IdProvider();
        var app = new AppNameProvider();
        var consumer = new SaveValidationConsumer<Item>(new TestPlanProvider(), repository, manual, idProvider, app);

        var harness = new InMemoryTestHarness();
        harness.Consumer(() => consumer);

        await harness.Start();
        try
        {
            await harness.InputQueueSendEndpoint.Send(new SaveRequested<Item>(new Item(5)));

            Assert.True(await harness.Published.Any<SaveValidated<Item>>());
        }
        finally
        {
            await harness.Stop();
        }
    }
}