# Validation

This repository contains a unified validation framework with fluent configuration APIs.

## Quick start

Register validation with Entity Framework:

```csharp
services.AddSetupValidation<Item>(builder =>
{
    builder.UseEntityFramework<MyDbContext>();
}, x => x.Metric);
```

Or with MongoDB:

```csharp
services.AddSetupValidation<Item>(b =>
    b.UseMongoDB("mongodb://localhost:27017", "validation"),
    x => x.Metric);
```

See `Validation.SampleApp` for a complete example.
