using MassTransit;
using MassTransit.Testing;
using Validation.Domain.Events;
using Validation.Domain.Validation;
using Serilog;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using Validation.Infrastructure.Messaging;
using Validation.Domain.Entities;

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
        var sink = new TestSink();
        var serilogger = new LoggerConfiguration().WriteTo.Sink(sink).CreateLogger();
        using var factory = LoggerFactory.Create(b => b.AddSerilog(serilogger));
        var logger = factory.CreateLogger<SaveValidationConsumer<Item>>();
        using var source = new ActivitySource("test-validation");
        var activities = new List<Activity>();
        using var listener = new ActivityListener
        {
            ShouldListenTo = s => s.Name == "test-validation",
            Sample = (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.AllData,
            ActivityStarted = a => activities.Add(a),
            ActivityStopped = _ => { }
        };
        ActivitySource.AddActivityListener(listener);

        var consumer = new SaveValidationConsumer<Item>(new TestPlanProvider(), repository, new SummarisationValidator(), logger, source);

        var harness = new InMemoryTestHarness();
        harness.Consumer(() => consumer);

        await harness.Start();
        try
        {
            await harness.InputQueueSendEndpoint.Send(new SaveRequested(Guid.NewGuid()));

            Assert.True(await harness.Published.Any<SaveValidated<Item>>());
            Assert.NotEmpty(sink.Events);
            Assert.NotEmpty(activities);
        }
        finally
        {
            await harness.Stop();
        }
    }
}