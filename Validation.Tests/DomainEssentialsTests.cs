using ValidationFlow.Domain;

namespace Validation.Tests;

public class DomainEssentialsTests
{
    [Fact]
    public void SaveAudit_properties_are_set_correctly()
    {
        var timestamp = DateTime.UtcNow;
        var id = Guid.NewGuid();
        var audit = new SaveAudit("Item", id, 10m, true, timestamp);

        Assert.Equal("Item", audit.EntityType);
        Assert.Equal(id, audit.EntityId);
        Assert.Equal(10m, audit.MetricValue);
        Assert.True(audit.Validated);
        Assert.Equal(timestamp, audit.Timestamp);
    }

    [Fact]
    public void ValidationPlan_properties_are_set_correctly()
    {
        Func<int, decimal> selector = x => x;
        var plan = new ValidationPlan<int>(selector, ThresholdType.PercentChange, 5m);

        Assert.Equal(selector, plan.MetricSelector);
        Assert.Equal(ThresholdType.PercentChange, plan.ThresholdType);
        Assert.Equal(5m, plan.ThresholdValue);
    }
}
