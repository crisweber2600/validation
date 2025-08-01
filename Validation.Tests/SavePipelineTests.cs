using MassTransit;
using MassTransit.Testing;
using Validation.Domain.Entities;
using ValidationFlow.Messages;
using Validation.Domain.Validation;
using Validation.Infrastructure.Messaging;
using Validation.Infrastructure.Repositories;
using Validation.Infrastructure;

namespace Validation.Tests;

public class SavePipelineTests
{
    private class TestPlanProvider : IValidationPlanProvider
    {
        public IEnumerable<IValidationRule> GetRules<T>() => Array.Empty<IValidationRule>();
        
        public ValidationPlan GetPlan(Type t) => new ValidationPlan(
            entity => ((Item)entity).Metric, 
            ThresholdType.RawDifference, 
            100m
        );
        
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
            => throw new Exception("Repository failure for testing");
        
        public Task<SaveAudit?> GetLastAsync(Guid entityId, CancellationToken ct = default)
        {
            var audit = Audits.Where(a => a.EntityId == entityId)
                .OrderByDescending(a => a.Timestamp)
                .FirstOrDefault();
            return Task.FromResult<SaveAudit?>(audit);
        }
    }

    [Fact]
    public async Task Pipeline_processes_save_request_without_fault()
    {
        var repo = new InMemorySaveAuditRepository();
        var harness = new InMemoryTestHarness();
        harness.Consumer(() => new SaveValidationConsumer<Item>(new TestPlanProvider(), repo, new SummarisationValidator()));
        harness.Consumer(() => new SaveCommitConsumer<Item>(repo));

        await harness.Start();
        try
        {
            var item = new Item(5);
            await harness.InputQueueSendEndpoint.Send(new SaveRequested<Item>("App", "Item", item.Id, item));

            Assert.True(await harness.Published.Any<SaveValidated<Item>>());
            Assert.False(await harness.Published.Any<SaveCommitFault<Item>>());
        }
        finally
        {
            await harness.Stop();
        }
    }

    [Fact]
    public async Task Pipeline_publishes_fault_when_repository_fails()
    {
        var repo = new FailingRepository();
        var harness = new InMemoryTestHarness();
        harness.Consumer(() => new SaveValidationConsumer<Item>(new TestPlanProvider(), repo, new SummarisationValidator()));
        harness.Consumer(() => new SaveCommitConsumer<Item>(repo));

        await harness.Start();
        try
        {
            var item = new Item(10);
            await harness.InputQueueSendEndpoint.Send(new SaveRequested<Item>("App", "Item", item.Id, item));

            Assert.True(await harness.Published.Any<SaveCommitFault<Item>>());
        }
        finally
        {
            await harness.Stop();
        }
    }

    [Fact]
    public async Task Pipeline_validation_consumer_publishes_save_validated_event()
    {
        var repo = new InMemorySaveAuditRepository();
        var harness = new InMemoryTestHarness();
        var validationConsumer = harness.Consumer(() => new SaveValidationConsumer<Item>(new TestPlanProvider(), repo, new SummarisationValidator()));

        await harness.Start();
        try
        {
            var item = new Item(7);
            await harness.InputQueueSendEndpoint.Send(new SaveRequested<Item>("App", "Item", item.Id, item));

            Assert.True(await validationConsumer.Consumed.Any<SaveRequested<Item>>());
            Assert.True(await harness.Published.Any<SaveValidated<Item>>());
        }
        finally
        {
            await harness.Stop();
        }
    }
}