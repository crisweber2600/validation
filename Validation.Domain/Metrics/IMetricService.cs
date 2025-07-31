using System.Linq.Expressions;

namespace Validation.Domain.Metrics;

public interface IMetricService
{
    Task<double> ComputeAsync<T>(IQueryable<T> source, Expression<Func<T, double>> selector, ValidationStrategy strategy);
}
