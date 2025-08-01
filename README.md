# validation

Example setup using the new fluent API:

```csharp
var services = new ServiceCollection();
services.SetupValidation(b =>
    b.UseSqlServer<TestDbContext>("Server=(local);Database=validation;Trusted_Connection=True;")
);
```

