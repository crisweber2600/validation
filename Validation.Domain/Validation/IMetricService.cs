using System.Linq.Expressions;

namespace Validation.Domain.Validation;

public interface IMetricService
{
    Task<double> ComputeAsync<T>(IQueryable<T> query, Expression<Func<T, double>> selector, ValidationStrategy strategy);
}
