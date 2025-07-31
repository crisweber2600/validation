using System.Collections.Generic;
using System.Linq;

namespace Validation.Domain.Validation;

public class ValidationPlan
{
    public IReadOnlyCollection<IValidationRule> Rules { get; }

    public ValidationPlan(IEnumerable<IValidationRule> rules)
    {
        Rules = rules.ToArray();
    }
}
