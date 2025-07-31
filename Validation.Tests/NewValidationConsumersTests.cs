using MassTransit;
using MassTransit.Testing;
using Validation.Domain.Entities;
using Validation.Domain.Events;
using Validation.Infrastructure.Services;
using Validation.Domain.Validation;
using Validation.Infrastructure.Messaging;
using Validation.Infrastructure.Repositories;
using Validation.Infrastructure;

namespace Validation.Tests;

public class NewValidationConsumersTests
{
    private class PlanProvider : IValidationPlanProvider
    {
        public IEnumerable<IValidationRule> GetPlan<T>()
        {
            yield return new RawDifferenceRule(100); // always valid
        }
    }

    private class FailingRepository : ISaveAuditRepository
    {
        public Task AddAsync(SaveAudit entity, CancellationToken ct = default) => Task.CompletedTask;
        public Task DeleteAsync(Guid id, CancellationToken ct = default) => Task.CompletedTask;
        public Task<SaveAudit?> GetAsync(Guid id, CancellationToken ct = default) => Task.FromResult<SaveAudit?>(null);
        public Task<SaveAudit?> GetLastForEntityAsync(Guid entityId, CancellationToken ct = default)
        {
            return Task.FromResult<SaveAudit?>(new SaveAudit { Id = Guid.NewGuid(), EntityId = entityId });
        }
        public Task UpdateAsync(SaveAudit entity, CancellationToken ct = default) => throw new Exception("fail");
    }

    [Fact]
    public async Task SaveValidationConsumer_publishes_SaveValidated()
    {
        var repository = new InMemorySaveAuditRepository();
        var planProvider = new PlanProvider();
        var validator = new SummarisationValidator();
        var consumer = new SaveValidationConsumer<Item>(planProvider, repository, validator);

        var harness = new InMemoryTestHarness();
        var consumerHarness = harness.Consumer(() => consumer);
        await harness.Start();
        try
        {
            var id = Guid.NewGuid();
            await harness.InputQueueSendEndpoint.Send(new SaveRequested(id));
            Assert.True(await consumerHarness.Consumed.Any<SaveRequested>());
            Assert.True(await harness.Published.Any<SaveValidated<Item>>());
            Assert.Single(repository.Audits);
        }
        finally
        {
            await harness.Stop();
        }
    }

    [Fact]
    public async Task SaveCommitConsumer_publishes_fault_on_error()
    {
        var repository = new FailingRepository();
        var consumer = new SaveCommitConsumer<Item>(repository);
        var harness = new InMemoryTestHarness();
        var consumerHarness = harness.Consumer(() => consumer);
        await harness.Start();
        try
        {
            await harness.InputQueueSendEndpoint.Send(new SaveValidated<Item>(Guid.NewGuid()));
            Assert.True(await consumerHarness.Consumed.Any<SaveValidated<Item>>());
            Assert.True(await harness.Published.Any<SaveCommitFault<Item>>());
        }
        finally
        {
            await harness.Stop();
        }
    }
}
