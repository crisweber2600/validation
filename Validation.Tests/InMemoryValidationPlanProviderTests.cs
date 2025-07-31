using Validation.Domain.Entities;
using Validation.Domain.Validation;
using Validation.Infrastructure.ValidationPlans;

namespace Validation.Tests;

public class InMemoryValidationPlanProviderTests
{
    [Fact]
    public void Returns_plan_for_registered_type()
    {
        var provider = new InMemoryValidationPlanProvider();
        var plan = new ValidationPlan(new[] { new AlwaysValidRule() });
        provider.AddPlan<Item>(plan);

        var result = provider.GetPlan(typeof(Item));

        Assert.Equal(plan, result);
    }
}
