using MassTransit;
using MassTransit.Testing;
using Validation.Domain.Events;
using Validation.Domain.Validation;
using Validation.Infrastructure.Messaging;
using Validation.Domain.Entities;
using Validation.Infrastructure.Repositories;
using Validation.Infrastructure;

namespace Validation.Tests;

public class SavePipelineTests
{
    private class TestPlanProvider : IValidationPlanProvider
    {
        public IEnumerable<IValidationRule> GetRules<T>() => new[] { new RawDifferenceRule(100) };
        public ValidationPlan GetPlan(Type t) => new ValidationPlan(new[] { new RawDifferenceRule(100) });
        public void AddPlan<T>(ValidationPlan plan) { }
    }

    private class FailingRepository : ISaveAuditRepository
    {
        private readonly InMemorySaveAuditRepository _inner = new();
        public Task AddAsync(SaveAudit entity, CancellationToken ct = default) => _inner.AddAsync(entity, ct);
        public Task DeleteAsync(Guid id, CancellationToken ct = default) => _inner.DeleteAsync(id, ct);
        public Task<SaveAudit?> GetAsync(Guid id, CancellationToken ct = default) => throw new Exception("fail");
        public Task UpdateAsync(SaveAudit entity, CancellationToken ct = default) => throw new Exception("fail");
        public Task<SaveAudit?> GetLastAsync(Guid entityId, CancellationToken ct = default) => _inner.GetLastAsync(entityId, ct);
    }

    [Fact]
    public async Task Save_requested_triggers_validation_and_commit()
    {
        var repo = new InMemorySaveAuditRepository();
        var validator = new SummarisationValidator();
        var harness = new InMemoryTestHarness();
        harness.Consumer(() => new SaveValidationConsumer<Item>(new TestPlanProvider(), repo, validator));
        harness.Consumer(() => new SaveCommitConsumer<Item>(repo));

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

    [Fact]
    public async Task Commit_failure_publishes_fault()
    {
        var repo = new FailingRepository();
        var validator = new SummarisationValidator();
        var harness = new InMemoryTestHarness();
        harness.Consumer(() => new SaveValidationConsumer<Item>(new TestPlanProvider(), repo, validator));
        harness.Consumer(() => new SaveCommitConsumer<Item>(repo));

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
}
