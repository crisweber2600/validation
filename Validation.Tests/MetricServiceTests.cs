using Validation.Domain.Validation;
using Xunit;
using System.Linq;
using System.Threading.Tasks;

public class MetricServiceTests
{
    [Theory]
    [InlineData(ValidationStrategy.Sum, 6)]
    [InlineData(ValidationStrategy.Average, 2)]
    [InlineData(ValidationStrategy.Count, 3)]
    [InlineData(ValidationStrategy.Variance, 2.0/3.0)]
    public async Task ComputeAsync_returns_expected_result(ValidationStrategy strategy, double expected)
    {
        var data = new[] {1.0, 2.0, 3.0}.AsQueryable();
        var service = new MetricService();
        var result = await service.ComputeAsync(data, x => x, strategy);
        Assert.Equal(expected, result, 5);
    }
}
