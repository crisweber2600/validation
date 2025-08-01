using MassTransit.Testing;
using MassTransit;
using Validation.Domain.Events;
using Validation.Infrastructure.Messaging;
using Validation.Infrastructure.Repositories;
using Validation.Infrastructure;
using Validation.Domain.Entities;
using Validation.Domain.Validation;
namespace Validation.Tests;

public class SavePipelineTests
{
    [Fact]
    public async Task Save_request_flows_through_validation_and_commit()
    {
        var repo = new InMemorySaveAuditRepository();
        var validationConsumer = new SaveValidationConsumer<Item>(new TestPlanProvider(), repo, new SummarisationValidator());
        var commitConsumer = new SaveCommitConsumer<Item>(repo);

        var harness = new InMemoryTestHarness();
        harness.Consumer(() => validationConsumer);
        harness.Consumer(() => commitConsumer);

        await harness.Start();
        try
        {
            await harness.InputQueueSendEndpoint.Send(new SaveRequested(Guid.NewGuid()));

            Assert.True(await harness.Published.Any<SaveValidated<Item>>());
            Assert.False(await harness.Published.Any<SaveCommitFault<Item>>());
        }
        finally
        {
            await harness.Stop();
        }
    }

    private class FailingRepository : ISaveAuditRepository
    {
        public Task AddAsync(SaveAudit entity, CancellationToken ct = default) => Task.CompletedTask;
        public Task DeleteAsync(Guid id, CancellationToken ct = default) => Task.CompletedTask;
        public Task<SaveAudit?> GetAsync(Guid id, CancellationToken ct = default) => throw new Exception("boom");
        public Task UpdateAsync(SaveAudit entity, CancellationToken ct = default) => Task.CompletedTask;
        public Task<SaveAudit?> GetLastAsync(Guid entityId, CancellationToken ct = default) => Task.FromResult<SaveAudit?>(null);
    }

    [Fact]
    public async Task Commit_fault_published_when_repository_throws()
    {
        var repo = new FailingRepository();
        var validationConsumer = new SaveValidationConsumer<Item>(new TestPlanProvider(), repo, new SummarisationValidator());
        var commitConsumer = new SaveCommitConsumer<Item>(repo);

        var harness = new InMemoryTestHarness();
        harness.Consumer(() => validationConsumer);
        harness.Consumer(() => commitConsumer);

        await harness.Start();
        try
        {
            await harness.InputQueueSendEndpoint.Send(new SaveRequested(Guid.NewGuid()));

            Assert.True(await harness.Published.Any<SaveCommitFault<Item>>());
        }
        finally
        {
            await harness.Stop();
        }
    }

    private class TestPlanProvider : IValidationPlanProvider
    {
        public IEnumerable<IValidationRule> GetRules<T>() => new[] { new RawDifferenceRule(100) };
        public ValidationPlan GetPlan(Type t) => new ValidationPlan(new[] { new RawDifferenceRule(100) });
        public void AddPlan<T>(ValidationPlan plan) { }
    }
}
