using Validation.Domain.Validation;

namespace Validation.Tests;

public class SummarisationValidatorListTests
{
    [Fact]
    public void Duplicate_items_fail_validation()
    {
        var validator = new SummarisationValidator();
        var rule = new DuplicateEqualityRule<string, string>();
        var items = new[] { "car", "jar", "car" };

        var result = validator.Validate(items, i => i, new[] { rule });

        Assert.False(result);
    }
}