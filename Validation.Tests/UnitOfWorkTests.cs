using Validation.Domain.Entities;
using Validation.Domain.Validation;
using Validation.Infrastructure;

namespace Validation.Tests;

public class UnitOfWorkTests
{
    [Fact]
    public async Task SaveChangesAsync_marks_entity_validated_based_on_rules()
    {
        var uow = new UnitOfWork(100m);
        var entity = new YourEntity { Metric = 100.09m };
        var ruleSet = new ValidationPlan(new IValidationRule[]
        {
            new PercentChangeRule(0.1m),
            new RawDifferenceRule(5m)
        });
        uow.Add(entity);
        await uow.SaveChangesAsync(ruleSet);
        Assert.True(entity.Validated);
    }
}
