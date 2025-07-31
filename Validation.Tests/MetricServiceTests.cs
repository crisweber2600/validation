using Validation.Domain.Metrics;
using Validation.Infrastructure.Metrics;
using Validation.Domain.Validation;

namespace Validation.Tests;

public class MetricServiceTests
{
    private class Sample { public double Value { get; set; } }

    private readonly IMetricService _service = new InMemoryMetricService();

    private static IQueryable<Sample> Previous => new List<Sample>
    {
        new() { Value = 1 },
        new() { Value = 2 },
        new() { Value = 3 }
    }.AsQueryable();

    private static IQueryable<Sample> Current => new List<Sample>
    {
        new() { Value = 1 },
        new() { Value = 2 },
        new() { Value = 3 },
        new() { Value = 4 }
    }.AsQueryable();

    [Fact]
    public async Task Sum_with_raw_difference_rule()
    {
        var plan = new ValidationPlan<Sample>(ValidationStrategy.Sum, s => s.Value, new RawDifferenceRule(3m));
        var previous = await _service.ComputeAsync(Previous, plan.Selector, plan.Strategy);
        var current = await _service.ComputeAsync(Current, plan.Selector, plan.Strategy);
        var result = SummarisationValidator.Validate(current, previous, plan);
        Assert.False(result);
    }

    [Fact]
    public async Task Sum_with_percent_change_rule()
    {
        var plan = new ValidationPlan<Sample>(ValidationStrategy.Sum, s => s.Value, new PercentChangeRule(100m));
        var previous = await _service.ComputeAsync(Previous, plan.Selector, plan.Strategy);
        var current = await _service.ComputeAsync(Current, plan.Selector, plan.Strategy);
        var result = SummarisationValidator.Validate(current, previous, plan);
        Assert.True(result);
    }

    [Fact]
    public async Task Average_with_raw_difference_rule()
    {
        var plan = new ValidationPlan<Sample>(ValidationStrategy.Average, s => s.Value, new RawDifferenceRule(1m));
        var previous = await _service.ComputeAsync(Previous, plan.Selector, plan.Strategy);
        var current = await _service.ComputeAsync(Current, plan.Selector, plan.Strategy);
        var result = SummarisationValidator.Validate(current, previous, plan);
        Assert.True(result);
    }

    [Fact]
    public async Task Average_with_percent_change_rule()
    {
        var plan = new ValidationPlan<Sample>(ValidationStrategy.Average, s => s.Value, new PercentChangeRule(20m));
        var previous = await _service.ComputeAsync(Previous, plan.Selector, plan.Strategy);
        var current = await _service.ComputeAsync(Current, plan.Selector, plan.Strategy);
        var result = SummarisationValidator.Validate(current, previous, plan);
        Assert.False(result);
    }

    [Fact]
    public async Task Count_with_raw_difference_rule()
    {
        var plan = new ValidationPlan<Sample>(ValidationStrategy.Count, s => s.Value, new RawDifferenceRule(0m));
        var previous = await _service.ComputeAsync(Previous, plan.Selector, plan.Strategy);
        var current = await _service.ComputeAsync(Current, plan.Selector, plan.Strategy);
        var result = SummarisationValidator.Validate(current, previous, plan);
        Assert.False(result);
    }

    [Fact]
    public async Task Count_with_percent_change_rule()
    {
        var plan = new ValidationPlan<Sample>(ValidationStrategy.Count, s => s.Value, new PercentChangeRule(50m));
        var previous = await _service.ComputeAsync(Previous, plan.Selector, plan.Strategy);
        var current = await _service.ComputeAsync(Current, plan.Selector, plan.Strategy);
        var result = SummarisationValidator.Validate(current, previous, plan);
        Assert.True(result);
    }

    [Fact]
    public async Task Variance_with_raw_difference_rule()
    {
        var plan = new ValidationPlan<Sample>(ValidationStrategy.Variance, s => s.Value, new RawDifferenceRule(1m));
        var previous = await _service.ComputeAsync(Previous, plan.Selector, plan.Strategy);
        var current = await _service.ComputeAsync(Current, plan.Selector, plan.Strategy);
        var result = SummarisationValidator.Validate(current, previous, plan);
        Assert.True(result);
    }

    [Fact]
    public async Task Variance_with_percent_change_rule()
    {
        var plan = new ValidationPlan<Sample>(ValidationStrategy.Variance, s => s.Value, new PercentChangeRule(50m));
        var previous = await _service.ComputeAsync(Previous, plan.Selector, plan.Strategy);
        var current = await _service.ComputeAsync(Current, plan.Selector, plan.Strategy);
        var result = SummarisationValidator.Validate(current, previous, plan);
        Assert.False(result);
    }
}
