using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Validation.Domain.Entities;
using Validation.Domain.Validation;
using Validation.Infrastructure;
using Validation.Domain;
using Xunit;

namespace Validation.Tests;

public class SequenceValidatorTests
{
    private class IdProvider : IEntityIdProvider
    {
        public string GetId<T>(T entity) => ((Item)(object)entity).Id.ToString();
    }

    [Fact]
    public async Task ValidateAsync_within_threshold_returns_true()
    {
        var repo = new InMemorySaveAuditRepository();
        var item = new Item(100m);
        await repo.AddAsync(new SaveAudit { EntityId = item.Id, Metric = 95m });

        var result = await SequenceValidator.ValidateAsync(
            item,
            i => i.Metric,
            repo,
            new IdProvider(),
            10m,
            ThresholdType.RawDifference,
            CancellationToken.None);

        Assert.True(result);
    }

    [Fact]
    public async Task ValidateAsync_exceeds_threshold_returns_false()
    {
        var repo = new InMemorySaveAuditRepository();
        var item = new Item(100m);
        await repo.AddAsync(new SaveAudit { EntityId = item.Id, Metric = 80m });

        var result = await SequenceValidator.ValidateAsync(
            item,
            i => i.Metric,
            repo,
            new IdProvider(),
            10m,
            ThresholdType.RawDifference,
            CancellationToken.None);

        Assert.False(result);
    }

    [Fact]
    public async Task ValidateBatchAsync_validates_each_item()
    {
        var repo = new InMemorySaveAuditRepository();
        var items = new[] { new Item(10m), new Item(20m) };
        foreach (var i in items)
            await repo.AddAsync(new SaveAudit { EntityId = i.Id, Metric = 10m });

        var result = await SequenceValidator.ValidateBatchAsync(
            items,
            i => i.Metric,
            repo,
            new IdProvider(),
            5m,
            ThresholdType.RawDifference,
            null,
            CancellationToken.None);

        Assert.False(result); // second item diff = 10
    }
}
