using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Validation.Domain.Entities;
using Validation.Domain.Validation;
using Validation.Infrastructure;
using Validation.Infrastructure.Repositories;
using Xunit;
using Validation.Domain;

namespace Validation.Tests;

public class SequenceValidatorTests
{
    private class IdProvider : IEntityIdProvider
    {
        public string GetId<T>(T entity) => ((Item)(object)entity).Id.ToString();
    }

    [Fact]
    public async Task ValidateAsync_NoHistory_ReturnsTrue()
    {
        var repo = new InMemorySaveAuditRepository();
        var item = new Item(10m);
        var result = await SequenceValidator.ValidateAsync(
            item,
            i => i.Metric,
            repo,
            new IdProvider(),
            15m,
            ThresholdType.RawDifference,
            CancellationToken.None);
        Assert.True(result);
    }

    [Fact]
    public async Task ValidateAsync_WithHistory_ExceedsThreshold_ReturnsFalse()
    {
        var repo = new InMemorySaveAuditRepository();
        var item = new Item(100m);
        await repo.AddAsync(new SaveAudit { EntityId = item.Id, Metric = 50m });
        var result = await SequenceValidator.ValidateAsync(
            item,
            i => i.Metric,
            repo,
            new IdProvider(),
            40m,
            ThresholdType.RawDifference,
            CancellationToken.None);
        Assert.False(result);
    }

    [Fact]
    public async Task ValidateBatchAsync_ValidSequence_ReturnsTrue()
    {
        var repo = new InMemorySaveAuditRepository();
        var items = new List<Item> { new Item(10m), new Item(12m) };
        var result = await SequenceValidator.ValidateBatchAsync(
            items,
            i => i.Metric,
            repo,
            new IdProvider(),
            10m,
            ThresholdType.RawDifference,
            _ => "00000000-0000-0000-0000-000000000000",
            CancellationToken.None);
        Assert.True(result);
    }

    [Fact]
    public async Task ValidateBatchAsync_InvalidSequence_ReturnsFalse()
    {
        var repo = new InMemorySaveAuditRepository();
        var item1 = new Item(10m);
        var item2 = new Item(20m);
        await repo.AddAsync(new SaveAudit { EntityId = item1.Id, Metric = 10m });
        var items = new List<Item> { item1, item2 };
        var result = await SequenceValidator.ValidateBatchAsync(
            items,
            i => i.Metric,
            repo,
            new IdProvider(),
            5m,
            ThresholdType.RawDifference,
            null,
            CancellationToken.None);
        Assert.False(result);
    }
}
