using System.Diagnostics;
using MassTransit.Testing;
using Microsoft.Extensions.Logging;
using Validation.Domain.Entities;
using Validation.Domain.Events;
using Validation.Domain.Validation;
using Validation.Infrastructure;
using Validation.Infrastructure.Messaging;

namespace Validation.Tests;

public class TestLogger<T> : ILogger<T>
{
    public List<string> Logs { get; } = new();
    public IDisposable BeginScope<TState>(TState state) => NullDisposable.Instance;
    public bool IsEnabled(LogLevel logLevel) => true;
    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        => Logs.Add(formatter(state, exception));
    private class NullDisposable : IDisposable { public static NullDisposable Instance { get; } = new(); public void Dispose() { } }
}

public class ConsumerLoggingTests
{
    private class TestPlanProvider : IValidationPlanProvider
    {
        public IEnumerable<IValidationRule> GetRules<T>() => new[] { new RawDifferenceRule(100) };
        public ValidationPlan GetPlan(Type t) => new ValidationPlan(new[] { new RawDifferenceRule(100) });
        public void AddPlan<T>(ValidationPlan plan) { }
    }


    private class TestActivityListener : IDisposable
    {
        public List<Activity> Started { get; } = new();
        private readonly ActivityListener _listener;
        public TestActivityListener(string sourceName)
        {
            _listener = new ActivityListener
            {
                ShouldListenTo = s => s.Name == sourceName,
                Sample = (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.AllData,
                ActivityStarted = a => Started.Add(a)
            };
            ActivitySource.AddActivityListener(_listener);
        }
        public void Dispose() => _listener.Dispose();
    }

    [Fact]
    public async Task SaveValidationConsumer_logs_and_traces()
    {
        var repo = new InMemorySaveAuditRepository();
        var logger = new TestLogger<SaveValidationConsumer<Item>>();
        var sourceName = "Validation.Infrastructure";
        using var listener = new TestActivityListener(sourceName);
        var consumer = new SaveValidationConsumer<Item>(new TestPlanProvider(), repo, new SummarisationValidator(), logger, new ActivitySource(sourceName));

        var harness = new InMemoryTestHarness();
        harness.Consumer(() => consumer);

        await harness.Start();
        try
        {
            await harness.InputQueueSendEndpoint.Send(new SaveRequested(Guid.NewGuid()));
            Assert.True(await harness.Published.Any<SaveValidated<Item>>());
            Assert.NotEmpty(logger.Logs);
            Assert.Contains(listener.Started, a => a.DisplayName == "SaveValidationConsumer.Consume");
        }
        finally
        {
            await harness.Stop();
        }
    }

    [Fact]
    public async Task SaveCommitConsumer_logs_and_traces()
    {
        var repo = new InMemorySaveAuditRepository();
        var logger = new TestLogger<SaveCommitConsumer<Item>>();
        var sourceName = "Validation.Infrastructure";
        using var listener = new TestActivityListener(sourceName);
        var consumer = new SaveCommitConsumer<Item>(repo, logger, new ActivitySource(sourceName));

        var harness = new InMemoryTestHarness();
        harness.Consumer(() => consumer);

        await harness.Start();
        try
        {
            var audit = new SaveAudit { Id = Guid.NewGuid(), EntityId = Guid.NewGuid() };
            await repo.AddAsync(audit);
            await harness.InputQueueSendEndpoint.Send(new SaveValidated<Item>(audit.EntityId, audit.Id));
            Assert.True(await harness.Consumed.Any<SaveValidated<Item>>());
            Assert.NotEmpty(logger.Logs);
            Assert.Contains(listener.Started, a => a.DisplayName == "SaveCommitConsumer.Consume");
        }
        finally
        {
            await harness.Stop();
        }
    }
}
