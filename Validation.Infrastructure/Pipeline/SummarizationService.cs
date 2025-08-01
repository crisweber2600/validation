using Validation.Domain.Validation;

namespace Validation.Infrastructure.Pipeline;

/// <summary>
/// Calculates a single metric value from a collection of gathered values.
/// </summary>
public class SummarizationService
{
    private readonly ValidationStrategy _strategy;

    public SummarizationService(ValidationStrategy strategy)
    {
        _strategy = strategy;
    }

    /// <summary>
    /// Produce the summary value for the supplied metrics.
    /// </summary>
    public decimal Summarize(IEnumerable<decimal> metrics)
    {
        var list = metrics as decimal[] ?? metrics.ToArray();
        if (!list.Any()) return 0m;
        return _strategy switch
        {
            ValidationStrategy.Sum => list.Sum(),
            ValidationStrategy.Average => list.Average(),
            ValidationStrategy.Count => list.Length,
            ValidationStrategy.Variance => CalculateVariance(list),
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    private static decimal CalculateVariance(decimal[] values)
    {
        var avg = values.Average();
        var diff = values.Select(v => (v - avg) * (v - avg)).Average();
        return diff;
    }
}
