using System.Linq.Expressions;
using Validation.Domain.Metrics;

namespace Validation.Infrastructure.Metrics;

public class InMemoryMetricService : IMetricService
{
    public Task<double> ComputeAsync<T>(IQueryable<T> source, Expression<Func<T, double>> selector, ValidationStrategy strategy)
    {
        var values = source.Select(selector.Compile()).ToList();
        double result = strategy switch
        {
            ValidationStrategy.Sum => values.Sum(),
            ValidationStrategy.Average => values.Average(),
            ValidationStrategy.Count => values.Count,
            ValidationStrategy.Variance => ComputeVariance(values),
            _ => throw new ArgumentOutOfRangeException(nameof(strategy))
        };
        return Task.FromResult(result);
    }

    private static double ComputeVariance(List<double> values)
    {
        if (values.Count == 0) return 0;
        var avg = values.Average();
        var variance = values.Select(v => (v - avg) * (v - avg)).Average();
        return variance;
    }
}
