using Xunit;
using System;
using System.Threading.Tasks;
using Validation.Domain.Entities;
using Validation.Domain.Validation;
using Validation.Infrastructure;

namespace Validation.Tests;

public class UnitOfWorkTests
{
    [Fact]
    public async Task SaveChanges_marks_entity_validated()
    {
        var uow = new UnitOfWork<YourEntity>();
        var entity = new YourEntity(105m);
        await uow.AddAsync(entity);

        var ruleSet = new EntityValidationRuleSet<YourEntity>(e => e.Metric,
            new PercentChangeRule(10m),
            new RawDifferenceRule(5m));

        await uow.SaveChangesAsync(ruleSet);

        Console.WriteLine($"Entity validated: {entity.Validated}");
        Assert.True(entity.Validated);
    }
}
