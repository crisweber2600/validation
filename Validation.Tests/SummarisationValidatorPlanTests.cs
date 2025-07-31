using Xunit;
using Validation.Domain.Validation;

namespace Validation.Tests;

public class SummarisationValidatorPlanTests
{
    [Fact]
    public void Validate_RawDifference_within_threshold_returns_true()
    {
        var validator = new SummarisationValidator();
        var plan = new ValidationPlan(_ => 0m, ThresholdType.RawDifference, 5m);

        var result = validator.Validate(10m, 14m, plan);

        Assert.True(result);
    }

    [Fact]
    public void Validate_PercentChange_within_threshold_returns_true()
    {
        var validator = new SummarisationValidator();
        var plan = new ValidationPlan(_ => 0m, ThresholdType.PercentChange, 10m);

        var result = validator.Validate(100m, 109m, plan);

        Assert.True(result);
    }
}
