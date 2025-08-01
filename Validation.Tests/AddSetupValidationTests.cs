using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Validation.Domain.Entities;
using Validation.Domain.Validation;
using Validation.Infrastructure.DI;
using Validation.Infrastructure.Messaging;

namespace Validation.Tests;

public class AddSetupValidationTests
{
    [Fact]
    public void AddSetupValidation_registers_plan_and_consumers()
    {
        var services = new ServiceCollection();
        services.AddSetupValidation<Item>(
            builder => builder.SetupDatabase<TestDbContext>("addsetup-test"),
            i => i.Metric,
            ThresholdType.RawDifference,
            1m);

        using var provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();

        Assert.NotNull(scope.ServiceProvider.GetService<SaveValidationConsumer<Item>>());
        Assert.NotNull(scope.ServiceProvider.GetService<SaveCommitConsumer<Item>>());
        Assert.NotNull(scope.ServiceProvider.GetService<TestDbContext>());

        var planProvider = scope.ServiceProvider.GetRequiredService<IValidationPlanProvider>();
        var plan = planProvider.GetPlan(typeof(Item));
        Assert.Equal(ThresholdType.RawDifference, plan.ThresholdType);
        Assert.Equal(1m, plan.ThresholdValue);
    }
}
