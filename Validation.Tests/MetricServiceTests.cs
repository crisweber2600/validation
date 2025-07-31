using Validation.Domain.Validation;
using Validation.Infrastructure.Metrics;

namespace Validation.Tests;

public class MetricServiceTests
{
    [Theory]
    [InlineData(ValidationStrategy.Sum, 6)]
    [InlineData(ValidationStrategy.Average, 2)]
    [InlineData(ValidationStrategy.Count, 3)]
    [InlineData(ValidationStrategy.Variance, 0.6666666666666666)]
    public async Task ComputeAsync_returns_expected_value(ValidationStrategy strategy, double expected)
    {
        var service = new InMemoryMetricService();
        var data = new[] {1d, 2d, 3d}.AsQueryable();
        var result = await service.ComputeAsync(data, x => x, strategy);
        Assert.Equal(expected, result, 10);
    }
}
