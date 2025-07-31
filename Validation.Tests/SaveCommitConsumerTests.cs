using MassTransit;
using MassTransit.Testing;
using Validation.Domain.Events;
using Validation.Infrastructure.Messaging;
using Validation.Infrastructure;
using Validation.Infrastructure.Repositories;
using Validation.Domain.Entities;

namespace Validation.Tests;

public class SaveCommitConsumerTests
{
    [Fact]
    public async Task Persist_audit_record_when_validated()
    {
        var repo = new InMemorySaveAuditRepository();
        var consumer = new SaveCommitConsumer(repo);

        var harness = new InMemoryTestHarness();
        harness.Consumer(() => consumer);

        await harness.Start();
        try
        {
            await harness.InputQueueSendEndpoint.Send(new SaveValidated(Guid.NewGuid(), true));

            Assert.True(await harness.Consumed.Any<SaveValidated>());
            Assert.Single(repo.Audits);
        }
        finally
        {
            await harness.Stop();
        }
    }
}