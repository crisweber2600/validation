using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Validation.Domain.Entities;
using Validation.Domain.Validation;
using Validation.Domain;
using Validation.Infrastructure;
using Validation.Tests;

namespace Validation.Tests;

public class SequenceValidatorTests
{
    private class TestIdProvider : IEntityIdProvider
    {
        public string GetId<T>(T entity) => ((Item)(object)entity).Id.ToString();
    }

    [Fact]
    public async Task ValidateAsync_ReturnsTrue_WhenDifferenceWithinThreshold()
    {
        var repo = new InMemorySaveAuditRepository();
        var item = new Item(110m);
        await repo.AddAsync(new SaveAudit { Id = Guid.NewGuid(), EntityId = item.Id, Metric = 100m }, CancellationToken.None);
        var result = await SequenceValidator.ValidateAsync(
            item,
            i => i.Metric,
            repo,
            new TestIdProvider(),
            15m,
            ThresholdType.RawDifference,
            CancellationToken.None);
        Assert.True(result);
    }

    [Fact]
    public async Task ValidateBatchAsync_ReturnsFalse_WhenAnyDifferenceExceedsThreshold()
    {
        var repo = new InMemorySaveAuditRepository();
        var items = new List<Item>
        {
            new Item(110m),
            new Item(160m)
        };
        await repo.AddAsync(new SaveAudit { Id = Guid.NewGuid(), EntityId = items[0].Id, Metric = 100m }, CancellationToken.None);
        await repo.AddAsync(new SaveAudit { Id = Guid.NewGuid(), EntityId = items[1].Id, Metric = 100m }, CancellationToken.None);
        var result = await SequenceValidator.ValidateBatchAsync(
            items,
            i => i.Metric,
            repo,
            new TestIdProvider(),
            50m,
            ThresholdType.RawDifference,
            null,
            CancellationToken.None);
        Assert.False(result);
    }
}
