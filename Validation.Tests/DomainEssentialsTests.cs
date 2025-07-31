using ValidationFlow.Domain;

namespace Validation.Tests;

public class DomainEssentialsTests
{
    [Fact]
    public void SaveAudit_record_properties_assigned()
    {
        var timestamp = DateTime.UtcNow;
        var audit = new SaveAudit("Item", Guid.NewGuid(), 1.23m, true, timestamp);
        Assert.Equal("Item", audit.EntityType);
        Assert.True(audit.Validated);
        Assert.Equal(1.23m, audit.MetricValue);
        Assert.Equal(timestamp, audit.Timestamp);
    }

    [Fact]
    public void ValidationPlan_initializes_properties()
    {
        Func<int, decimal> selector = x => x;
        var plan = new ValidationPlan<int>(selector, ThresholdType.RawDifference, 5m);
        Assert.Equal(selector, plan.MetricSelector);
        Assert.Equal(ThresholdType.RawDifference, plan.ThresholdType);
        Assert.Equal(5m, plan.ThresholdValue);
    }
}
