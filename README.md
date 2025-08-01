# validation

Example configuration using `SetupValidationBuilder`:

```csharp
var services = new ServiceCollection();
services.SetupValidation(builder =>
{
    builder.UseSqlServer<MyDbContext>("<connection-string>");
});
```