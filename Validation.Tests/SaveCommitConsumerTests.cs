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
    private class FailingRepository : ISaveAuditRepository
    {
        public Task AddAsync(SaveAudit entity, CancellationToken ct = default) => Task.CompletedTask;
        public Task DeleteAsync(Guid id, CancellationToken ct = default) => Task.CompletedTask;
        public Task<SaveAudit?> GetAsync(Guid id, CancellationToken ct = default) => Task.FromResult<SaveAudit?>(new SaveAudit { Id = id.ToString(), EntityId = id.ToString() });
        public Task UpdateAsync(SaveAudit entity, CancellationToken ct = default) => throw new Exception("fail");
        public Task<SaveAudit?> GetLastAsync(string entityId, CancellationToken ct = default) => Task.FromResult<SaveAudit?>(null);
        public Task<SaveAudit?> GetLastAsync(Guid entityId, CancellationToken ct = default) => GetLastAsync(entityId.ToString(), ct);
        public Task<IEnumerable<SaveAudit>> GetByEntityTypeAsync(string entityType, CancellationToken ct = default) => Task.FromResult<IEnumerable<SaveAudit>>(Enumerable.Empty<SaveAudit>());
        public Task<IEnumerable<SaveAudit>> GetByApplicationAsync(string applicationName, CancellationToken ct = default) => Task.FromResult<IEnumerable<SaveAudit>>(Enumerable.Empty<SaveAudit>());
        public Task<IEnumerable<SaveAudit>> GetByTimeRangeAsync(DateTime from, DateTime to, CancellationToken ct = default) => Task.FromResult<IEnumerable<SaveAudit>>(Enumerable.Empty<SaveAudit>());
        public Task<IEnumerable<SaveAudit>> GetByCorrelationIdAsync(string correlationId, CancellationToken ct = default) => Task.FromResult<IEnumerable<SaveAudit>>(Enumerable.Empty<SaveAudit>());
        public Task<SaveAudit?> GetLastAuditAsync(string entityId, string propertyName, CancellationToken ct = default) => Task.FromResult<SaveAudit?>(null);
        public Task AddOrUpdateAuditAsync(string entityId, string entityType, string propertyName, decimal propertyValue, bool isValid, string? applicationName = null, string? operationType = null, string? correlationId = null, CancellationToken ct = default) => Task.CompletedTask;
    }

    [Fact]
    public async Task Publish_SaveCommitFault_on_error()
    {
        var repo = new FailingRepository();
        var consumer = new SaveCommitConsumer<Item>(repo);

        var harness = new InMemoryTestHarness();
        harness.Consumer(() => consumer);

        await harness.Start();
        try
        {
            await harness.InputQueueSendEndpoint.Send(new SaveValidated<Item>(Guid.NewGuid(), Guid.NewGuid()));

            Assert.True(await harness.Published.Any<SaveCommitFault<Item>>());
        }
        finally
        {
            await harness.Stop();
        }
    }
}