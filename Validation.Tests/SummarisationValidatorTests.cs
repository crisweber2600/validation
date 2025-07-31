using Validation.Domain.Validation;
using Xunit;
using System.Linq;
using System.Threading.Tasks;

public class SummarisationValidatorTests
{
    public static TheoryData<ValidationStrategy, bool> RawDifferenceData => new()
    {
        { ValidationStrategy.Sum, false },
        { ValidationStrategy.Average, true },
        { ValidationStrategy.Count, true },
        { ValidationStrategy.Variance, true }
    };

    public static TheoryData<ValidationStrategy, bool> PercentChangeData => new()
    {
        { ValidationStrategy.Sum, true },
        { ValidationStrategy.Average, true },
        { ValidationStrategy.Count, true },
        { ValidationStrategy.Variance, true }
    };

    [Theory]
    [MemberData(nameof(RawDifferenceData))]
    public async Task Validate_with_raw_difference_rule(ValidationStrategy strategy, bool expected)
    {
        var prevData = new[] {1.0, 2.0, 3.0}.AsQueryable();
        var curData = new[] {2.0, 3.0, 4.0}.AsQueryable();
        var service = new MetricService();
        var prevMetric = await service.ComputeAsync(prevData, x => x, strategy);
        var curMetric = await service.ComputeAsync(curData, x => x, strategy);

        var plan = new ValidationPlan<double>(x => x, strategy, new RawDifferenceRule(2));
        var result = SummarisationValidator.Validate(curMetric, prevMetric, plan);
        Assert.Equal(expected, result);
    }

    [Theory]
    [MemberData(nameof(PercentChangeData))]
    public async Task Validate_with_percent_change_rule(ValidationStrategy strategy, bool expected)
    {
        var prevData = new[] {1.0, 2.0, 3.0}.AsQueryable();
        var curData = new[] {2.0, 3.0, 4.0}.AsQueryable();
        var service = new MetricService();
        var prevMetric = await service.ComputeAsync(prevData, x => x, strategy);
        var curMetric = await service.ComputeAsync(curData, x => x, strategy);

        var plan = new ValidationPlan<double>(x => x, strategy, new PercentChangeRule(50));
        var result = SummarisationValidator.Validate(curMetric, prevMetric, plan);
        Assert.Equal(expected, result);
    }
}
