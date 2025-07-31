using Validation.Domain.Validation;
using Validation.Infrastructure.Metrics;

namespace Validation.Tests;

public class SummarisationValidatorTests
{
    private readonly InMemoryMetricService _service = new();
    private readonly IQueryable<double> _previous = new[] {10d, 20d, 30d}.AsQueryable();
    private readonly IQueryable<double> _current = new[] {11d, 22d, 33d}.AsQueryable();

    [Theory]
    [InlineData(ValidationStrategy.Sum, true)]
    [InlineData(ValidationStrategy.Average, true)]
    [InlineData(ValidationStrategy.Count, true)]
    [InlineData(ValidationStrategy.Variance, false)]
    public async Task Validate_with_raw_difference_rule(ValidationStrategy strategy, bool expected)
    {
        var plan = new ValidationPlan<double>(x => x, strategy, new RawDifferenceRule(10));
        var prev = await _service.ComputeAsync(_previous, x => x, strategy);
        var curr = await _service.ComputeAsync(_current, x => x, strategy);
        var result = SummarisationValidator.Validate(curr, prev, plan);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(ValidationStrategy.Sum, true)]
    [InlineData(ValidationStrategy.Average, true)]
    [InlineData(ValidationStrategy.Count, true)]
    [InlineData(ValidationStrategy.Variance, false)]
    public async Task Validate_with_percent_change_rule(ValidationStrategy strategy, bool expected)
    {
        var plan = new ValidationPlan<double>(x => x, strategy, new PercentChangeRule(15));
        var prev = await _service.ComputeAsync(_previous, x => x, strategy);
        var curr = await _service.ComputeAsync(_current, x => x, strategy);
        var result = SummarisationValidator.Validate(curr, prev, plan);
        Assert.Equal(expected, result);
    }
}
