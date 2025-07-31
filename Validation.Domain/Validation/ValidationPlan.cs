namespace Validation.Domain.Validation;

public class ValidationPlan
{
    public IEnumerable<IValidationRule> Rules { get; }

    public ValidationPlan(IEnumerable<IValidationRule> rules)
    {
        Rules = rules;
    }
}
