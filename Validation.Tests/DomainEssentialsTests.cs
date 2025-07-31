using ValidationFlow.Domain;

namespace Validation.Tests;

public class DomainEssentialsTests
{
    [Fact]
    public void SaveAudit_record_holds_values()
    {
        var timestamp = DateTime.UtcNow;
        var id = Guid.NewGuid();
        var audit = new SaveAudit("Item", id, 42.5m, true, timestamp);
        Assert.Equal("Item", audit.EntityType);
        Assert.Equal(id, audit.EntityId);
        Assert.Equal(42.5m, audit.MetricValue);
        Assert.True(audit.Validated);
        Assert.Equal(timestamp, audit.Timestamp);
    }

    [Fact]
    public void ValidationPlan_record_holds_configuration()
    {
        Func<string, decimal> selector = s => s.Length;
        var plan = new ValidationPlan<string>(selector, ThresholdType.RawDifference, 1.5m);
        Assert.Equal(selector, plan.MetricSelector);
        Assert.Equal(ThresholdType.RawDifference, plan.ThresholdType);
        Assert.Equal(1.5m, plan.ThresholdValue);
    }
}
