using System.Linq.Expressions;
using Validation.Domain.Validation;

namespace Validation.Infrastructure.Metrics;

public class InMemoryMetricService : IMetricService
{
    public Task<double> ComputeAsync<T>(IQueryable<T> source, Expression<Func<T, double>> selector, ValidationStrategy strategy)
    {
        var values = source.Select(selector).ToList();
        double result = strategy switch
        {
            ValidationStrategy.Sum => values.Sum(),
            ValidationStrategy.Average => values.Count == 0 ? 0 : values.Average(),
            ValidationStrategy.Count => values.Count,
            ValidationStrategy.Variance => values.Count == 0 ? 0 : CalculateVariance(values),
            _ => throw new ArgumentOutOfRangeException(nameof(strategy), strategy, null)
        };
        return Task.FromResult(result);
    }

    private static double CalculateVariance(IList<double> values)
    {
        var mean = values.Average();
        var variance = values.Sum(v => Math.Pow(v - mean, 2)) / values.Count;
        return variance;
    }
}
