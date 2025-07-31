using System.Linq;
using Validation.Domain.Validation;

namespace Validation.Tests;

public class SummarisationValidatorRuleSetTests
{
    private record Data(double Metric);

    [Fact]
    public void ValidateRuleSet_supports_multiple_rules()
    {
        var validator = new SummarisationValidator();
        var data = new[] { new Data(1), new Data(2), new Data(3) }.AsQueryable();
        var rules = new ValidationRuleSet<Data>(d => d.Metric,
            new ValidationRule(ValidationStrategy.Sum, 6),
            new ValidationRule(ValidationStrategy.Average, 2));
        Assert.True(validator.ValidateRuleSet(data, rules));
    }

    [Fact]
    public void ValidateRuleSet_fails_when_threshold_exceeded()
    {
        var validator = new SummarisationValidator();
        var data = new[] { new Data(1), new Data(2), new Data(3) }.AsQueryable();
        var rules = new ValidationRuleSet<Data>(d => d.Metric,
            new ValidationRule(ValidationStrategy.Sum, 5));
        Assert.False(validator.ValidateRuleSet(data, rules));
    }

    [Fact]
    public void ValidateRuleSet_supports_count_and_variance()
    {
        var validator = new SummarisationValidator();
        var data = new[] { new Data(1), new Data(2), new Data(3), new Data(4) }.AsQueryable();
        var rules = new ValidationRuleSet<Data>(d => d.Metric,
            new ValidationRule(ValidationStrategy.Count, 4),
            new ValidationRule(ValidationStrategy.Variance, 1.25));
        Assert.True(validator.ValidateRuleSet(data, rules));
    }
}
