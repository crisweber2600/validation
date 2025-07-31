using Validation.Domain.Validation;

namespace Validation.Tests;

public class SummarisationValidatorTests
{
    [Fact]
    public void DuplicateEqualityRule_fails_on_duplicates()
    {
        var items = new[] { "car", "jar", "car" };
        var validator = new SummarisationValidator();
        var rules = new IListValidationRule<string, string>[] { new DuplicateEqualityRule<string, string>() };

        var result = validator.Validate(items, x => x, rules);

        Assert.False(result);
    }
}
