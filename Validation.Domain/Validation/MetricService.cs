using System.Linq.Expressions;

namespace Validation.Domain.Validation;

public class MetricService : IMetricService
{
    public Task<double> ComputeAsync<T>(IQueryable<T> query, Expression<Func<T, double>> selector, ValidationStrategy strategy)
    {
        var list = query.Select(selector.Compile()).ToList();
        double result = strategy switch
        {
            ValidationStrategy.Sum => list.Sum(),
            ValidationStrategy.Average => list.Average(),
            ValidationStrategy.Count => list.Count,
            ValidationStrategy.Variance =>
                list.Count > 0
                    ? list.Select(v => v - list.Average()).Select(d => d * d).Average()
                    : 0,
            _ => throw new NotImplementedException()
        };
        return Task.FromResult(result);
    }
}
