# Unified Validation System

This repository demonstrates a fluent configuration API for registering validation flows and infrastructure services.

```csharp
services.AddSetupValidation<Item>(builder =>
{
    builder.UseEntityFramework<AppDbContext>();
}, item => item.Metric);
```

See `Validation.SampleApp` for a runnable example.
