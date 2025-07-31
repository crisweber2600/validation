using Validation.Domain.Validation;

namespace Validation.Tests;

public class SummarisationValidatorPlanTests
{
    [Fact]
    public void RawDifference_under_threshold_returns_true()
    {
        var validator = new SummarisationValidator();
        var plan = new ValidationPlan(_ => 0m, ThresholdType.RawDifference, 5m);
        Assert.True(validator.Validate(10m, 12m, plan));
    }

    [Fact]
    public void PercentChange_over_threshold_returns_false()
    {
        var validator = new SummarisationValidator();
        var plan = new ValidationPlan(_ => 0m, ThresholdType.PercentChange, 10m);
        Assert.False(validator.Validate(100m, 120m, plan));
    }
}
