# Validation

This repository demonstrates a unified validation system. The `AddSetupValidation<T>` extension
provides a quick way to configure a database and a summarisation plan in a single call.

```csharp
services.AddSetupValidation<Item>(
    builder => builder.UseEntityFramework<SampleDbContext>(o => o.UseInMemoryDatabase("sample")),
    item => item.Metric,
    ThresholdType.GreaterThan,
    5m);
```

This registers an EF Core context, a validation plan using the `Metric` property and the
necessary MassTransit consumers for save validation flows.
