# Validation

This repository contains a small sample project demonstrating a validation workflow.

## Configuring Validation

The infrastructure can be configured with a fluent API using `SetupValidation` and `SetupValidationBuilder`.

```csharp
services.SetupValidation(builder =>
{
    builder.UseSqlServer<TestDbContext>("Server=.;Database=Validation;Trusted_Connection=True;");
});
```

For MongoDB:

```csharp
var mongoClient = new MongoClient(connectionString);
var database = mongoClient.GetDatabase("validation");
services.SetupValidation(builder => builder.UseMongo(database));
```

The builder registers the appropriate database services and audit repositories.
