using MassTransit;
using MassTransit.Testing;
using Validation.Domain.Entities;
using Validation.Domain.Events;
using Validation.Domain.Validation;
using Validation.Domain.Providers;
using Validation.Infrastructure.Messaging;
using Validation.Infrastructure.Repositories;
using Validation.Infrastructure;

namespace Validation.Tests;

public class SavePipelineTests
{
    private class MockEntityIdProvider : IEntityIdProvider
    {
        public Guid GetEntityId<T>(T entity) => Guid.NewGuid();
        public bool CanHandle<T>() => true;
    }

    private class MockApplicationNameProvider : IApplicationNameProvider
    {
        public string GetApplicationName() => "TestApp";
        public string GetApplicationName(string? context) => "TestApp";
    }

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
            var idString = id.ToString();
            Audits.RemoveAll(a => a.Id == idString);
            return Task.CompletedTask;
        }
        
        public Task<SaveAudit?> GetAsync(Guid id, CancellationToken ct = default)
        {
            var idString = id.ToString();
            return Task.FromResult<SaveAudit?>(Audits.FirstOrDefault(a => a.Id == idString));
        }
        
        public Task UpdateAsync(SaveAudit entity, CancellationToken ct = default)
            => throw new Exception("Repository failure for testing");
        
        public Task<SaveAudit?> GetLastAsync(string entityId, CancellationToken ct = default)
        {
            var audit = Audits.Where(a => a.EntityId == entityId)
                .OrderByDescending(a => a.Timestamp)
                .FirstOrDefault();
            return Task.FromResult<SaveAudit?>(audit);
        }
        
        public Task<SaveAudit?> GetLastAsync(Guid entityId, CancellationToken ct = default)
        {
            return GetLastAsync(entityId.ToString(), ct);
        }
        
        public Task<IEnumerable<SaveAudit>> GetByEntityTypeAsync(string entityType, CancellationToken ct = default) 
            => Task.FromResult<IEnumerable<SaveAudit>>(Enumerable.Empty<SaveAudit>());
        
        public Task<IEnumerable<SaveAudit>> GetByApplicationAsync(string applicationName, CancellationToken ct = default) 
            => Task.FromResult<IEnumerable<SaveAudit>>(Enumerable.Empty<SaveAudit>());
        
        public Task<IEnumerable<SaveAudit>> GetByTimeRangeAsync(DateTime from, DateTime to, CancellationToken ct = default) 
            => Task.FromResult<IEnumerable<SaveAudit>>(Enumerable.Empty<SaveAudit>());
        
        public Task<IEnumerable<SaveAudit>> GetByCorrelationIdAsync(string correlationId, CancellationToken ct = default) 
            => Task.FromResult<IEnumerable<SaveAudit>>(Enumerable.Empty<SaveAudit>());
    }

    [Fact]
    public async Task Pipeline_processes_save_request_without_fault()
    {
        var repo = new InMemorySaveAuditRepository();
        var harness = new InMemoryTestHarness();
        harness.Consumer(() => new SaveValidationConsumer<Item>(new TestPlanProvider(), repo, new SummarisationValidator(), new MockEntityIdProvider(), new MockApplicationNameProvider()));
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
    public async Task Pipeline_publishes_fault_when_repository_fails()
    {
        var repo = new FailingRepository();
        var harness = new InMemoryTestHarness();
        harness.Consumer(() => new SaveValidationConsumer<Item>(new TestPlanProvider(), repo, new SummarisationValidator(), new MockEntityIdProvider(), new MockApplicationNameProvider()));
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

    [Fact]
    public async Task Pipeline_validation_consumer_publishes_save_validated_event()
    {
        var repo = new InMemorySaveAuditRepository();
        var harness = new InMemoryTestHarness();
        var validationConsumer = harness.Consumer(() => new SaveValidationConsumer<Item>(new TestPlanProvider(), repo, new SummarisationValidator(), new MockEntityIdProvider(), new MockApplicationNameProvider()));

        await harness.Start();
        try
        {
            await harness.InputQueueSendEndpoint.Send(new SaveRequested(Guid.NewGuid()));

            Assert.True(await validationConsumer.Consumed.Any<SaveRequested>());
            Assert.True(await harness.Published.Any<SaveValidated<Item>>());
        }
        finally
        {
            await harness.Stop();
        }
    }
}