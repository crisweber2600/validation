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
        public Task AddAsync(SaveAudit entity, CancellationToken ct = default) => Task.CompletedTask;
        public Task DeleteAsync(Guid id, CancellationToken ct = default) => Task.CompletedTask;
        public Task<SaveAudit?> GetAsync(Guid id, CancellationToken ct = default) => Task.FromResult<SaveAudit?>(new SaveAudit { Id = id, EntityId = id });
        public Task UpdateAsync(SaveAudit entity, CancellationToken ct = default) => throw new Exception("fail");
        public Task<SaveAudit?> GetLastAsync(Guid entityId, CancellationToken ct = default) => Task.FromResult<SaveAudit?>(null);
    }

    [Fact]
    public async Task Pipeline_publishes_validated_and_no_fault()
    {
        var repository = new InMemorySaveAuditRepository();
        var validationConsumer = new SaveValidationConsumer<Item>(new TestPlanProvider(), repository, new SummarisationValidator());
        var commitConsumer = new SaveCommitConsumer<Item>(repository);

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

    [Fact]
    public async Task Pipeline_publishes_fault_when_commit_fails()
    {
        var repository = new FailingRepository();
        var validationConsumer = new SaveValidationConsumer<Item>(new TestPlanProvider(), repository, new SummarisationValidator());
        var commitConsumer = new SaveCommitConsumer<Item>(repository);

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
}
