using System.Linq.Expressions;
using Validation.Domain.Validation;
using Validation.Domain.Metrics;
using Validation.Infrastructure.Services;

namespace Validation.Tests;

public class SummarisationValidatorTests
{
    private readonly IMetricService _service = new InMemoryMetricService();

    private class DataItem { public double Value { get; set; } }

    [Theory]
    [InlineData(ValidationStrategy.Sum, 5, true)]
    [InlineData(ValidationStrategy.Average, 0.5, false)]
    [InlineData(ValidationStrategy.Count, 0, false)]
    [InlineData(ValidationStrategy.Variance, 3, false)]
    public async Task RawDifference_rule_validates_correctly(ValidationStrategy strategy, double threshold, bool expected)
    {
        var previous = GetPreviousData(strategy);
        var current = GetCurrentData(strategy);
        var selector = (Expression<Func<DataItem,double>>)(x => x.Value);
        var plan = new ValidationPlan<DataItem>(selector, strategy, new RawDifferenceRule((decimal)threshold));

        var prevMetric = await _service.ComputeAsync(previous.AsQueryable(), selector, strategy);
        var currMetric = await _service.ComputeAsync(current.AsQueryable(), selector, strategy);

        var result = SummarisationValidator.Validate(currMetric, prevMetric, plan);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(ValidationStrategy.Sum, 10, false)]
    [InlineData(ValidationStrategy.Average, 60, true)]
    [InlineData(ValidationStrategy.Count, 60, true)]
    [InlineData(ValidationStrategy.Variance, 700, true)]
    public async Task PercentChange_rule_validates_correctly(ValidationStrategy strategy, double threshold, bool expected)
    {
        var previous = GetPreviousData(strategy);
        var current = GetCurrentData(strategy);
        var selector = (Expression<Func<DataItem,double>>)(x => x.Value);
        var plan = new ValidationPlan<DataItem>(selector, strategy, new PercentChangeRule((decimal)threshold));

        var prevMetric = await _service.ComputeAsync(previous.AsQueryable(), selector, strategy);
        var currMetric = await _service.ComputeAsync(current.AsQueryable(), selector, strategy);

        var result = SummarisationValidator.Validate(currMetric, prevMetric, plan);
        Assert.Equal(expected, result);
    }

    private static IEnumerable<DataItem> GetPreviousData(ValidationStrategy strategy)
    {
        return strategy switch
        {
            ValidationStrategy.Sum => new[] { new DataItem { Value = 10 }, new DataItem { Value = 20 } },
            ValidationStrategy.Average => new[] { new DataItem { Value = 1 }, new DataItem { Value = 2 }, new DataItem { Value = 3 } },
            ValidationStrategy.Count => new[] { new DataItem { Value = 1 }, new DataItem { Value = 2 } },
            ValidationStrategy.Variance => new[] { new DataItem { Value = 1 }, new DataItem { Value = 2 }, new DataItem { Value = 3 } },
            _ => throw new NotSupportedException()
        };
    }

    private static IEnumerable<DataItem> GetCurrentData(ValidationStrategy strategy)
    {
        return strategy switch
        {
            ValidationStrategy.Sum => new[] { new DataItem { Value = 15 }, new DataItem { Value = 20 } },
            ValidationStrategy.Average => new[] { new DataItem { Value = 1 }, new DataItem { Value = 2 }, new DataItem { Value = 6 } },
            ValidationStrategy.Count => new[] { new DataItem { Value = 1 }, new DataItem { Value = 2 }, new DataItem { Value = 3 } },
            ValidationStrategy.Variance => new[] { new DataItem { Value = 1 }, new DataItem { Value = 2 }, new DataItem { Value = 6 } },
            _ => throw new NotSupportedException()
        };
    }
}
