using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Validation.Domain.Validation;

namespace Validation.Infrastructure.Pipeline;

/// <summary>
/// Aggregates a collection of metric values into a single summary value.
/// </summary>
public class SummarizationService
{
    private readonly IValidationService _validationService;

    /// <summary>
    /// Creates the service with the validation component used later in the pipeline.
    /// </summary>
    public SummarizationService(IValidationService validationService)
    {
        _validationService = validationService;
    }

    /// <summary>
    /// Compute the summary value using the supplied strategy.
    /// </summary>
    public Task<decimal> SummarizeAsync(IEnumerable<decimal> metrics, ValidationStrategy strategy, CancellationToken ct = default)
    {
        var list = metrics.ToList();
        decimal result = strategy switch
        {
            ValidationStrategy.Sum => list.Sum(),
            ValidationStrategy.Average => list.Count == 0 ? 0m : list.Average(),
            ValidationStrategy.Count => list.Count,
            ValidationStrategy.Variance => ComputeVariance(list),
            _ => throw new ArgumentOutOfRangeException(nameof(strategy))
        };
        return Task.FromResult(result);
    }

    private static decimal ComputeVariance(IReadOnlyList<decimal> values)
    {
        if (values.Count == 0)
            return 0m;
        var avg = values.Average();
        var diff = values.Select(v => Math.Pow((double)(v - avg), 2)).Average();
        return (decimal)diff;
    }
}
