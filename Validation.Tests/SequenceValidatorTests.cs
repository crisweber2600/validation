using System.Collections.Generic;
using System.Threading.Tasks;
using Validation.Domain.Entities;
using Validation.Domain.Validation;
using Validation.Infrastructure.Repositories;


using Validation.Infrastructure;
using Validation.Domain;
namespace Validation.Tests;

public class SequenceValidatorTests
{
    private class IdProvider : IEntityIdProvider
    {
        public string GetId<T>(T entity) => ((Item)(object)entity).Id.ToString();
    }

    [Fact]
    public async Task ValidateAsync_NoPreviousRecord_ReturnsTrue()
    {
        var repo = new InMemorySaveAuditRepository();
        var item = new Item(50m);
        var result = await SequenceValidator.ValidateAsync(
            item,
            i => i.Metric,
            repo,
            new IdProvider(),
            10m,
            ThresholdType.RawDifference,
            default);
        Assert.True(result);
    }

    [Fact]
    public async Task ValidateAsync_ChangeExceedsThreshold_ReturnsFalse()
    {
        var repo = new InMemorySaveAuditRepository();
        var item = new Item(100m);
        await repo.AddAsync(new SaveAudit { EntityId = item.Id, Metric = 100m }, default);
        item.UpdateMetric(120m);
        var result = await SequenceValidator.ValidateAsync(
            item,
            i => i.Metric,
            repo,
            new IdProvider(),
            10m,
            ThresholdType.RawDifference,
            default);
        Assert.False(result);
    }

    [Fact]
    public async Task ValidateBatchAsync_WhenAnyItemInvalid_ReturnsFalse()
    {
        var repo = new InMemorySaveAuditRepository();
        var a = new Item(100m);
        var b = new Item(50m);
        await repo.AddAsync(new SaveAudit { EntityId = a.Id, Metric = 100m }, default);
        await repo.AddAsync(new SaveAudit { EntityId = b.Id, Metric = 50m }, default);
        a.UpdateMetric(105m);
        b.UpdateMetric(70m); // exceeds
        var items = new[] { a, b };
        var result = await SequenceValidator.ValidateBatchAsync(
            items,
            i => i.Metric,
            repo,
            new IdProvider(),
            10m,
            ThresholdType.RawDifference,
            null,
            default);
        Assert.False(result);
    }
}
