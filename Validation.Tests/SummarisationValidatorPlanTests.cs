using Validation.Domain.Validation;

namespace Validation.Tests;

public class SummarisationValidatorPlanTests
{
    [Fact]
    public void Validate_RawDifference_within_threshold_returns_true()
    {
        var plan = new ValidationPlan(_ => 0m, ThresholdType.RawDifference, 10m);
        var validator = new SummarisationValidator();
        Assert.True(validator.Validate(100m, 105m, plan));
    }

    [Fact]
    public void Validate_RawDifference_exceeds_threshold_returns_false()
    {
        var plan = new ValidationPlan(_ => 0m, ThresholdType.RawDifference, 5m);
        var validator = new SummarisationValidator();
        Assert.False(validator.Validate(100m, 120m, plan));
    }

    [Fact]
    public void Validate_PercentChange_within_threshold_returns_true()
    {
        var plan = new ValidationPlan(_ => 0m, ThresholdType.PercentChange, 10m);
        var validator = new SummarisationValidator();
        Assert.True(validator.Validate(100m, 105m, plan));
    }

    [Fact]
    public void Validate_PercentChange_exceeds_threshold_returns_false()
    {
        var plan = new ValidationPlan(_ => 0m, ThresholdType.PercentChange, 10m);
        var validator = new SummarisationValidator();
        Assert.False(validator.Validate(100m, 120m, plan));
    }
}