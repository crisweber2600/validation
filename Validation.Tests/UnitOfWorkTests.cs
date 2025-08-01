using Microsoft.EntityFrameworkCore;
using Validation.Domain.Entities;
using Validation.Domain.Validation;
using Validation.Infrastructure;

namespace Validation.Tests;

public class UnitOfWorkTests
{
    [Fact]
    public async Task SaveChangesAsync_marks_entity_validated()
    {
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase("uowtest")
            .Options;
        using var context = new TestDbContext(options);
        var validator = new SummarisationValidator();
        var uow = new UnitOfWork(context, validator);

        var entity = new Measurement(3m);
        context.Measurements.Add(entity);

        var ruleSet = new ValidationPlan(new IValidationRule[]
        {
            new PercentChangeRule(0.1m),
            new RawDifferenceRule(5m)
        });

        await uow.SaveChangesAsync<Measurement>(ruleSet);

        Assert.True(entity.Validated);
    }
}
