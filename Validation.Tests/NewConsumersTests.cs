using MassTransit;
using MassTransit.Testing;
using Validation.Domain.Events;
using Validation.Domain.Validation;
using Validation.Infrastructure;
using Validation.Infrastructure.Messaging;
using Validation.Infrastructure.Repositories;

namespace Validation.Tests;

public class NewConsumersTests
{
    private class FakePlanProvider : IValidationPlanProvider
    {
        public IEnumerable<IValidationRule> GetPlan<T>() => new[] { new RawDifferenceRule(100) };
    }

    private class ThrowingRepository : ISaveAuditRepository
    {
        public Task AddAsync(SaveAudit entity, CancellationToken ct = default) => throw new InvalidOperationException("fail");
        public Task DeleteAsync(Guid id, CancellationToken ct = default) => Task.CompletedTask;
        public Task<SaveAudit?> GetAsync(Guid id, CancellationToken ct = default) => Task.FromResult<SaveAudit?>(null);
        public Task UpdateAsync(SaveAudit entity, CancellationToken ct = default) => throw new InvalidOperationException("fail");
    }

    [Fact]
    public async Task Save_validation_publishes_SaveValidated()
    {
        var repository = new InMemorySaveAuditRepository();
        var provider = new FakePlanProvider();
        var validator = new SummarisationValidator();
        var consumer = new SaveValidationConsumer<object>(provider, repository, validator);

        var harness = new InMemoryTestHarness();
        var consumerHarness = harness.Consumer(() => consumer);
        await harness.Start();
        try
        {
            await harness.InputQueueSendEndpoint.Send(new SaveRequested(Guid.NewGuid()));
            Assert.True(await consumerHarness.Consumed.Any<SaveRequested>());
            Assert.True(await harness.Published.Any<SaveValidated<object>>());
        }
        finally
        {
            await harness.Stop();
        }
    }

    [Fact]
    public async Task Save_commit_fault_is_published_on_error()
    {
        var repository = new ThrowingRepository();
        var consumer = new SaveCommitConsumer<object>(repository);

        var harness = new InMemoryTestHarness();
        var consumerHarness = harness.Consumer(() => consumer);
        await harness.Start();
        try
        {
            await harness.InputQueueSendEndpoint.Send(new SaveValidated<object>(Guid.NewGuid(), true));
            Assert.True(await consumerHarness.Consumed.Any<SaveValidated<object>>());
            Assert.True(await harness.Published.Any<SaveCommitFault<object>>());
        }
        finally
        {
            await harness.Stop();
        }
    }
}
