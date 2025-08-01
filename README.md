# validation

This repository demonstrates a unified validation system with a fluent setup API.

## Usage

Register validation with a metric selector using the new `AddSetupValidation<T>` extension:

```csharp
services.AddSetupValidation<Item>(builder =>
{
    builder.UseEntityFramework<MyDbContext>();
    builder.AddRule<Item>(i => i.Metric > 0);
}, i => i.Metric);
```

See `Validation.SampleApp/Program.cs` for a complete example.
