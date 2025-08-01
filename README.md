# Unified Validation System

This repository demonstrates a configurable validation framework. The `SetupValidationBuilder`
allows fluent configuration of Entity Framework or MongoDB persistence and registration
of validation flows.

The `AddSetupValidation<T>` extension sets up the infrastructure, registers a generic
repository and adds a validation plan using a metric selector.

```csharp
services.AddSetupValidation<Item>(builder => builder
    .UseEntityFramework<SampleDbContext>(o => o.UseInMemoryDatabase("sample"))
    .AddValidationFlow<Item>(flow => flow.EnableSaveValidation()),
    i => i.Metric);
```
