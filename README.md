# validation

This repository demonstrates a unified validation system. Services can register validation flows and database providers using the fluent API.

## Quick Example

```csharp
services.AddSetupValidation<Item>(
    b => b.UseEntityFramework<AppDbContext>(),
    item => item.Metric,
    ThresholdType.GreaterThan,
    5m);
```
