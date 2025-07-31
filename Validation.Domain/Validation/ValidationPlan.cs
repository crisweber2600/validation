namespace Validation.Domain.Validation;

public record ValidationPlan(IEnumerable<IValidationRule> Rules);
