namespace Validation.Infrastructure.Services;

using System.Collections.Generic;
using Validation.Domain.Validation;

public class SummarisationValidator
{
    public bool Validate(decimal previousMetric, decimal newMetric, IEnumerable<IValidationRule> rules)
    {
        foreach (var rule in rules)
        {
            if (!rule.Validate(previousMetric, newMetric))
                return false;
        }
        return true;
    }
}
