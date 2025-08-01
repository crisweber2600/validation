using MassTransit;
using MassTransit.Testing;
using Validation.Domain.Entities;
using Validation.Domain.Events;
using Validation.Domain.Validation;
using Validation.Infrastructure.Messaging;
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
        public List<SaveAudit> Audits { get; } = new();
        public Task AddAsync(SaveAudit entity, CancellationToken ct = default)
        {
            Audits.Add(entity);
            return Task.CompletedTask;
        }
        public Task DeleteAsync(Guid id, CancellationToken ct = default)
        {
            Audits.RemoveAll(a => a.Id == id);
            return Task.CompletedTask;
        }
        public Task<SaveAudit?> GetAsync(Guid id, CancellationToken ct = default)
        {
            return Task.FromResult<SaveAudit?>(Audits.FirstOrDefault(a => a.Id == id));
        }
        public Task UpdateAsync(SaveAudit entity, CancellationToken ct = default)
            => throw new Exception("fail");
        public Task<SaveAudit?> GetLastAsync(Guid entityId, CancellationToken ct = default)
        {
            var audit = Audits.Where(a => a.EntityId == entityId)
                .OrderByDescending(a => a.Timestamp)
                .FirstOrDefault();
            return Task.FromResult<SaveAudit?>(audit);
        }
    }

    [Fact]
    public async Task Pipeline_processes_without_fault()
    {
        var repo = new InMemorySaveAuditRepository();
        var harness = new InMemoryTestHarness();
        harness.Consumer(() => new SaveValidationConsumer<Item>(new TestPlanProvider(), repo, new SummarisationValidator()));
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
    public async Task Pipeline_publishes_fault_on_commit_error()
    {
        var repo = new FailingRepository();
        var harness = new InMemoryTestHarness();
        harness.Consumer(() => new SaveValidationConsumer<Item>(new TestPlanProvider(), repo, new SummarisationValidator()));
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
