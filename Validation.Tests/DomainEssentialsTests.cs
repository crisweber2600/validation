using ValidationFlow.Domain;

namespace Validation.Tests;

public class DomainEssentialsTests
{
    [Fact]
    public void SaveAudit_properties_round_trip()
    {
        var now = DateTime.UtcNow;
        var audit = new SaveAudit("Item", Guid.NewGuid(), 1.23m, true, now);
        Assert.Equal("Item", audit.EntityType);
        Assert.Equal(1.23m, audit.MetricValue);
        Assert.True(audit.Validated);
        Assert.Equal(now, audit.Timestamp);
    }

    [Fact]
    public void ValidationPlan_stores_configuration()
    {
        Func<string, decimal> selector = s => s.Length;
        var plan = new ValidationPlan<string>(selector, ThresholdType.RawDifference, 5);
        Assert.Equal(selector, plan.MetricSelector);
        Assert.Equal(ThresholdType.RawDifference, plan.ThresholdType);
        Assert.Equal(5, plan.ThresholdValue);
    }
}
