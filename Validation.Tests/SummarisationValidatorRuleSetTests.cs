using System.Linq;
using Validation.Domain.Validation;

namespace Validation.Tests;

public class SummarisationValidatorRuleSetTests
{
    [Fact]
    public void ValidateRuleSet_returns_true_when_all_rules_pass()
    {
        var data = new double[] {1,2,3,4,5}.AsQueryable();
        var ruleSet = new ValidationRuleSet<double>(x => x,
            new ValidationRule(ValidationStrategy.Sum, 20),
            new ValidationRule(ValidationStrategy.Average, 4),
            new ValidationRule(ValidationStrategy.Count, 6),
            new ValidationRule(ValidationStrategy.Variance, 2));
        var validator = new SummarisationValidator();

        Assert.True(validator.ValidateRuleSet(data, ruleSet));
    }

    [Fact]
    public void ValidateRuleSet_returns_false_when_any_rule_fails()
    {
        var data = new double[] {1,2,3,4,5}.AsQueryable();
        var ruleSet = new ValidationRuleSet<double>(x => x,
            new ValidationRule(ValidationStrategy.Sum, 10));
        var validator = new SummarisationValidator();

        Assert.False(validator.ValidateRuleSet(data, ruleSet));
    }
}
