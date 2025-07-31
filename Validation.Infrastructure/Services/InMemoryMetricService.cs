using System.Linq.Expressions;
using Validation.Domain.Metrics;

namespace Validation.Infrastructure.Services;

public class InMemoryMetricService : IMetricService
{
    public Task<double> ComputeAsync<T>(IQueryable<T> source, Expression<Func<T, double>> selector, ValidationStrategy strategy)
    {
        double result = strategy switch
        {
            ValidationStrategy.Sum => source.Sum(selector),
            ValidationStrategy.Average => source.Average(selector),
            ValidationStrategy.Count => source.Count(),
            ValidationStrategy.Variance => ComputeVariance(source.Select(selector)),
            _ => throw new ArgumentOutOfRangeException(nameof(strategy))
        };
        return Task.FromResult(result);
    }

    private static double ComputeVariance(IEnumerable<double> values)
    {
        var list = values.ToList();
        if (list.Count == 0) return 0;
        var mean = list.Average();
        var variance = list.Sum(v => Math.Pow(v - mean, 2)) / list.Count;
        return variance;
    }
}
