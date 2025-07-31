using Validation.Domain.Validation;

namespace Validation.Tests;

public class InMemoryValidationPlanProviderTests
{
    [Fact]
    public void Added_plan_can_be_retrieved()
    {
        var provider = new InMemoryValidationPlanProvider();
        var plan = new ValidationPlan(new[] { new RawDifferenceRule(1) });
        provider.AddPlan<string>(plan);

        var result = provider.GetPlan(typeof(string));

        Assert.Same(plan, result);
    }
}
