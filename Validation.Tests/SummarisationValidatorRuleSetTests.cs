using System.Linq;
using Validation.Domain.Validation;
using Xunit;

namespace Validation.Tests;

public class SummarisationValidatorRuleSetTests
{
    private class Sample { public double Value { get; set; } }

    [Fact]
    public void ValidateRuleSet_returns_true_when_all_rules_pass()
    {
        var data = new[]
        {
            new Sample { Value = 1 },
            new Sample { Value = 2 },
            new Sample { Value = 3 },
            new Sample { Value = 4 }
        }.AsQueryable();

        var ruleSet = new ValidationRuleSet<Sample>(s => s.Value,
            new ValidationRule(ValidationStrategy.Sum, 15),
            new ValidationRule(ValidationStrategy.Average, 4),
            new ValidationRule(ValidationStrategy.Count, 4));

        var validator = new SummarisationValidator();
        Assert.True(validator.ValidateRuleSet(data, ruleSet));
    }

    [Fact]
    public void ValidateRuleSet_returns_false_when_any_rule_fails()
    {
        var data = new[]
        {
            new Sample { Value = 10 },
            new Sample { Value = 5 }
        }.AsQueryable();

        var ruleSet = new ValidationRuleSet<Sample>(s => s.Value,
            new ValidationRule(ValidationStrategy.Sum, 10),
            new ValidationRule(ValidationStrategy.Average, 10));

        var validator = new SummarisationValidator();
        Assert.False(validator.ValidateRuleSet(data, ruleSet));
    }
}
