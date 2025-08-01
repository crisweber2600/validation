# Repository Pattern Integration with Unified Validation System

This sample project demonstrates how to integrate the Unified Validation System into an existing repository pattern implementation. It shows the progression from basic repositories to validated repositories and business services.

## Project Structure

```
Validation.RepositoryPattern.Sample/
??? Models/                     # Domain entities
?   ??? Product.cs             # Product entity with business logic
?   ??? Customer.cs            # Customer entity with business logic
??? Data/                      # Data access layer
?   ??? SampleDbContext.cs     # Entity Framework DbContext
??? Repositories/              # Repository pattern implementation
?   ??? IRepository.cs         # Generic repository interface
?   ??? IProductRepository.cs  # Product-specific repository interface
?   ??? ICustomerRepository.cs # Customer-specific repository interface
?   ??? Implementations/       # Repository implementations
?       ??? Repository.cs      # Basic generic repository
?       ??? ProductRepository.cs         # Basic product repository
?       ??? CustomerRepository.cs       # Basic customer repository
?       ??? ValidatedRepository.cs      # Validated generic repository
?       ??? ValidatedProductRepository.cs    # Validated product repository
?       ??? ValidatedCustomerRepository.cs   # Validated customer repository
??? Services/                  # Business services layer
?   ??? ProductService.cs      # Product business service
?   ??? CustomerService.cs     # Customer business service
??? Program.cs                 # Main demonstration program
```

## Key Features Demonstrated

### 1. Repository Pattern Layers

The sample demonstrates a clean separation of concerns:

- **Entities**: Domain models with business logic
- **Repositories**: Data access abstraction with both basic and validated implementations
- **Services**: Business logic layer that coordinates repositories and enforces business rules

### 2. Validation Integration

Shows two approaches to validation integration:

#### Basic Repositories (Without Validation)
- Direct data access without validation
- Suitable for internal operations or when validation is handled elsewhere
- Example: `ProductRepository`, `CustomerRepository`

#### Validated Repositories (With Validation Integration)
- Automatic validation on all modification operations (`Add`, `Update`, `SaveChanges`)
- Detailed validation error reporting with rule names
- Graceful handling of validation failures
- Example: `ValidatedProductRepository`, `ValidatedCustomerRepository`

### 3. Validation Rules Configuration

The sample configures comprehensive validation rules for both entities:

#### Product Validation Rules
```csharp
.AddRule<Product>("PositivePrice", product => product.Price > 0)
.AddRule<Product>("NonNegativeQuantity", product => product.Quantity >= 0)
.AddRule<Product>("RequiredName", product => !string.IsNullOrWhiteSpace(product.Name))
.AddRule<Product>("RequiredCategory", product => !string.IsNullOrWhiteSpace(product.Category))
.AddRule<Product>("ReasonablePrice", product => product.Price <= 100000)
.AddRule<Product>("ReasonableQuantity", product => product.Quantity <= 10000)
```

#### Customer Validation Rules
```csharp
.AddRule<Customer>("RequiredFirstName", customer => !string.IsNullOrWhiteSpace(customer.FirstName))
.AddRule<Customer>("ValidEmail", customer => !string.IsNullOrWhiteSpace(customer.Email) && customer.Email.Contains('@'))
.AddRule<Customer>("NonNegativeCreditLimit", customer => customer.CreditLimit >= 0)
.AddRule<Customer>("BalanceWithinCreditLimit", customer => customer.CurrentBalance <= customer.CreditLimit)
.AddRule<Customer>("ReasonableAge", customer => customer.Age >= 18 && customer.Age <= 120)
```

### 4. Business Service Integration

Business services demonstrate how to:
- Use validated repositories for data access
- Implement additional business logic validation
- Handle validation exceptions gracefully
- Coordinate multiple repository operations

## Running the Sample

### Prerequisites
- .NET 8 SDK
- Visual Studio 2022 or VS Code

### Steps
1. Navigate to the project directory:
   ```bash
   cd Validation.RepositoryPattern.Sample
   ```

2. Restore dependencies:
   ```bash
   dotnet restore
   ```

3. Run the sample:
   ```bash
   dotnet run
   ```

## Demonstration Scenarios

The sample runs through four key scenarios:

### 1. Basic Repository Pattern
- Shows standard repository operations without validation
- Demonstrates direct data access
- Used for comparison with validated approach

### 2. Validated Repository Pattern
- Shows automatic validation during repository operations
- Demonstrates validation failure handling
- Shows detailed error reporting with rule names

### 3. Business Services
- Shows how business services use validated repositories
- Demonstrates business logic validation in addition to entity validation
- Shows coordinated operations across multiple entities

### 4. Validation Failure Scenarios
- Tests various validation failure conditions
- Shows proper error handling and reporting
- Demonstrates how different validation rules are triggered

## Integration Points

### Service Registration

The sample shows how to register both basic and validated repositories:

```csharp
// Basic repositories (without validation)
services.AddScoped<ProductRepository>();
services.AddScoped<CustomerRepository>();

// Validated repositories (with validation integration)
services.AddScoped<ValidatedProductRepository>();
services.AddScoped<ValidatedCustomerRepository>();

// Use validated versions via interfaces
services.AddScoped<IProductRepository, ValidatedProductRepository>();
services.AddScoped<ICustomerRepository, ValidatedCustomerRepository>();
```

### Validation System Configuration

Shows comprehensive validation system setup:

```csharp
services.AddSetupValidation()
    .UseEntityFramework<SampleDbContext>()
    .AddValidationFlow<Product>(flow => flow
        .EnableSaveValidation()
        .EnableDeleteValidation()
        .EnableSoftDelete()
        .WithThreshold(x => x.Price, ThresholdType.GreaterThan, 0)
        .EnableAuditing())
    .AddRule<Product>("PositivePrice", product => product.Price > 0)
    // ... more rules
    .Build();
```

### Error Handling

Demonstrates proper validation error handling:

```csharp
try
{
    await repository.AddAsync(entity);
    await repository.SaveChangesAsync();
}
catch (ValidationException ex)
{
    // Handle validation failures with detailed rule information
    logger.LogError("Validation failed: {FailedRules}", 
        string.Join(", ", ex.FailedRules));
}
```

## Benefits of This Approach

1. **Separation of Concerns**: Validation is cleanly separated from business logic
2. **Flexibility**: Can choose validated or non-validated repositories based on needs
3. **Consistency**: Validation rules are applied consistently across all operations
4. **Maintainability**: Centralized validation rules are easy to update
5. **Testability**: Each layer can be tested independently
6. **Performance**: Validation can be bypassed for internal operations when needed

## Next Steps

This sample provides a foundation for integrating the validation system into your existing repository pattern. You can:

1. Adapt the entity models to match your domain
2. Customize validation rules for your business requirements  
3. Add more sophisticated business logic to the services
4. Implement additional repository patterns (Unit of Work, Specification, etc.)
5. Add caching, logging, or other cross-cutting concerns

## Related Documentation

- [Unified Validation System Documentation](../UNIFIED_VALIDATION_SYSTEM.md)
- [Validation Examples](../Validation.Examples.cs)
- [Repository Pattern Best Practices](https://docs.microsoft.com/en-us/dotnet/architecture/microservices/microservice-ddd-cqrs-patterns/infrastructure-persistence-layer-design)