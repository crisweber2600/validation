using Validation.Domain.Validation;

namespace Validation.Infrastructure.Pipeline;

/// <summary>
/// Calculates a summary metric from a sequence of values.
/// </summary>
public class SummarizationService
{
    private readonly ValidationStrategy _strategy;

    public SummarizationService(ValidationStrategy strategy = ValidationStrategy.Average)
    {
        _strategy = strategy;
    }

    /// <summary>
    /// Reduce the provided metrics to a single value using the configured strategy.
    /// </summary>
    public Task<decimal> SummarizeAsync(IEnumerable<decimal> metrics, CancellationToken ct = default)
    {
        var data = metrics.ToArray();
        decimal result = _strategy switch
        {
            ValidationStrategy.Sum => data.Sum(),
            ValidationStrategy.Average => data.Average(),
            ValidationStrategy.Count => data.Length,
            ValidationStrategy.Variance => CalculateVariance(data),
            _ => 0m
        };
        return Task.FromResult(result);
    }

    private static decimal CalculateVariance(decimal[] values)
    {
        if (values.Length == 0) return 0m;
        var avg = values.Average();
        var variance = values.Select(v => (double)(v - avg) * (double)(v - avg)).Average();
        return (decimal)variance;
    }
}
