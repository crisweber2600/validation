using System;
using System.Threading.Tasks;
using Validation.Domain.Entities;
using Validation.Domain.Validation;

class UnitOfWork
{
    private readonly SummarisationValidator _validator = new();
    public bool Validated { get; private set; }

    public Task SaveChangesAsync(ValidationPlan plan, Item entity)
    {
        Validated = _validator.Validate(0m, entity.Metric, plan);
        return Task.CompletedTask;
    }
}

class Program
{
    static async Task Main()
    {
        Console.WriteLine("Validation Sample Application");
        Console.WriteLine("=============================");

        // Create a validation plan using ThresholdType
        var plan = new ValidationPlan(
            entity => ((Item)entity).Metric, 
            ThresholdType.PercentChange, 
            0.1m
        );

        var entity = new Item(10);
        var uow = new UnitOfWork();
        
        await uow.SaveChangesAsync(plan, entity);
        
        Console.WriteLine($"Entity Metric: {entity.Metric}");
        Console.WriteLine($"Validation Result: {uow.Validated}");
        
        // Test with different value
        var entity2 = new Item(15);
        await uow.SaveChangesAsync(plan, entity2);
        Console.WriteLine($"Entity2 Metric: {entity2.Metric}");
        Console.WriteLine($"Validation Result 2: {uow.Validated}");
    }
}