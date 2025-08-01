# validation

## Configuring Validation Infrastructure

Use `SetupValidation` to fluently configure the database provider and validation plans:

```csharp
var services = new ServiceCollection();
services.SetupValidation(builder =>
{
    builder.UseSqlServer<MyDbContext>("<connection string>");
    builder.AddPlan<Item>(new ValidationPlan(e => ((Item)e).Metric, ThresholdType.PercentChange, 0.2m));
});
```