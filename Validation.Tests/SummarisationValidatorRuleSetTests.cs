using Validation.Domain.Validation;

namespace Validation.Tests;

public class SummarisationValidatorRuleSetTests
{
    private record NumericItem(double Metric);

    [Fact]
    public void ValidateRuleSet_returns_true_when_all_rules_pass()
    {
        var data = new[] { new NumericItem(1), new NumericItem(2), new NumericItem(3) }.AsQueryable();
        var ruleSet = new ValidationRuleSet<NumericItem>(x => x.Metric,
            new ValidationRule(ValidationStrategy.Sum, 6),
            new ValidationRule(ValidationStrategy.Average, 3),
            new ValidationRule(ValidationStrategy.Count, 3),
            new ValidationRule(ValidationStrategy.Variance, 1));

        var validator = new SummarisationValidator();
        Assert.True(validator.ValidateRuleSet(data, ruleSet));
    }

    [Fact]
    public void ValidateRuleSet_returns_false_if_any_rule_fails()
    {
        var data = new[] { new NumericItem(1), new NumericItem(2), new NumericItem(3) }.AsQueryable();
        var ruleSet = new ValidationRuleSet<NumericItem>(x => x.Metric,
            new ValidationRule(ValidationStrategy.Sum, 5));

        var validator = new SummarisationValidator();
        Assert.False(validator.ValidateRuleSet(data, ruleSet));
    }
}
