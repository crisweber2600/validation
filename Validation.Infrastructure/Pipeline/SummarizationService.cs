using Validation.Domain.Validation;

namespace Validation.Infrastructure.Pipeline;

public class SummarizationService
{
    private readonly ValidationStrategy _strategy;

    public SummarizationService(ValidationStrategy strategy)
    {
        _strategy = strategy;
    }

    public decimal Summarize(IEnumerable<decimal> metrics)
    {
        var values = metrics.ToArray();
        return _strategy switch
        {
            ValidationStrategy.Sum => values.Sum(),
            ValidationStrategy.Average => values.Length == 0 ? 0m : values.Average(),
            ValidationStrategy.Count => values.Length,
            ValidationStrategy.Variance => ComputeVariance(values),
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    private static decimal ComputeVariance(decimal[] values)
    {
        if (values.Length == 0) return 0m;
        var avg = values.Average();
        return (decimal)values.Select(v => Math.Pow((double)(v - avg), 2)).Average();
    }
}
