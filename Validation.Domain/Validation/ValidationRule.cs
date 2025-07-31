namespace Validation.Domain.Validation;

public record ValidationRule(ValidationStrategy Strategy, double Threshold);
