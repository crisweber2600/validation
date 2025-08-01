using MassTransit;
using MassTransit.Testing;
using Validation.Domain.Events;
using Validation.Domain.Validation;
using Validation.Infrastructure.Messaging;
using Validation.Domain.Entities;
using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace Validation.Tests;

public class SaveValidationConsumerTests
{
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
        var logger = new TestLogger<SaveValidationConsumer<Item>>();
        var source = new ActivitySource("test");
        var consumer = new SaveValidationConsumer<Item>(new TestPlanProvider(), repository, new SummarisationValidator(), logger, source);

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

    [Fact]
    public async Task Logs_validation_information()
    {
        var repository = new InMemorySaveAuditRepository();
        var logger = new TestLogger<SaveValidationConsumer<Item>>();
        var source = new ActivitySource("logtest");
        var consumer = new SaveValidationConsumer<Item>(new TestPlanProvider(), repository, new SummarisationValidator(), logger, source);

        var harness = new InMemoryTestHarness();
        harness.Consumer(() => consumer);

        var activities = new List<Activity>();
        using var listener = new ActivityListener
        {
            ShouldListenTo = s => true,
            Sample = (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.AllData,
            ActivityStarted = a => activities.Add(a)
        };
        ActivitySource.AddActivityListener(listener);

        await harness.Start();
        try
        {
            var id = Guid.NewGuid();
            await harness.InputQueueSendEndpoint.Send(new SaveRequested(id));

            Assert.True(logger.Messages.Any());
            Assert.True(activities.Any());
        }
        finally
        {
            await harness.Stop();
        }
    }
}