using MassTransit;
using MassTransit.Testing;
using Validation.Domain.Events;
using Validation.Infrastructure.Messaging;
using Validation.Infrastructure;
using Validation.Infrastructure.Repositories;
using Validation.Domain.Entities;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace Validation.Tests;

public class SaveCommitConsumerTests
{
    private class FailingRepository : ISaveAuditRepository
    {
        public Task AddAsync(SaveAudit entity, CancellationToken ct = default) => Task.CompletedTask;
        public Task DeleteAsync(Guid id, CancellationToken ct = default) => Task.CompletedTask;
        public Task<SaveAudit?> GetAsync(Guid id, CancellationToken ct = default) => Task.FromResult<SaveAudit?>(new SaveAudit { Id = id, EntityId = id });
        public Task UpdateAsync(SaveAudit entity, CancellationToken ct = default) => throw new Exception("fail");
        public Task<SaveAudit?> GetLastAsync(Guid entityId, CancellationToken ct = default) => Task.FromResult<SaveAudit?>(null);
    }

    [Fact]
    public async Task Publish_SaveCommitFault_on_error()
    {
        var repo = new FailingRepository();
        var logger = new TestLogger<SaveCommitConsumer<Item>>();
        var src = new ActivitySource("commit");
        var consumer = new SaveCommitConsumer<Item>(repo, logger, src);

        var harness = new InMemoryTestHarness();
        harness.Consumer(() => consumer);

        await harness.Start();
        try
        {
            await harness.InputQueueSendEndpoint.Send(new SaveValidated<Item>(Guid.NewGuid(), Guid.NewGuid()));

            Assert.True(await harness.Published.Any<SaveCommitFault<Item>>());
            Assert.Contains(logger.Messages, m => m.Contains("Failed"));
        }
        finally
        {
            await harness.Stop();
        }
    }

    [Fact]
    public async Task Emits_activity_span()
    {
        var repo = new FailingRepository();
        var logger = new TestLogger<SaveCommitConsumer<Item>>();
        var src = new ActivitySource("commit2");
        var consumer = new SaveCommitConsumer<Item>(repo, logger, src);

        var activities = new List<Activity>();
        using var listener = new ActivityListener
        {
            ShouldListenTo = s => true,
            Sample = (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.AllData,
            ActivityStarted = a => activities.Add(a)
        };
        ActivitySource.AddActivityListener(listener);

        var harness = new InMemoryTestHarness();
        harness.Consumer(() => consumer);

        await harness.Start();
        try
        {
            await harness.InputQueueSendEndpoint.Send(new SaveValidated<Item>(Guid.NewGuid(), Guid.NewGuid()));

            Assert.True(activities.Any());
        }
        finally
        {
            await harness.Stop();
        }
    }
}