using System;
using System.Threading.Tasks;
using Validation.Domain.Entities;
using Validation.Domain.Validation;

class UnitOfWork
{
    private readonly SummarisationValidator _validator = new();
    public bool Validated { get; private set; }

    public Task SaveChangesAsync(ValidationPlan ruleSet, Item entity)
    {
        Validated = _validator.Validate(0m, entity.Metric, ruleSet);
        return Task.CompletedTask;
    }
}

class Program
{
    static async Task Main()
    {
        var ruleSet = new ValidationPlan(new IValidationRule[]
        {
            new PercentChangeRule(0.1m),
            new RawDifferenceRule(5m)
        });
        var entity = new Item(10);
        var uow = new UnitOfWork();
        await uow.SaveChangesAsync(ruleSet, entity);
        Console.WriteLine($"Validated: {uow.Validated}");
    }
}
