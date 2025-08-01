# Validation.SampleApp

A comprehensive sample application demonstrating the usage and capabilities of the Unified Validation System.

## Overview

This console application provides practical examples of how to configure, initialize, and use the Unified Validation System in real-world scenarios. It demonstrates basic and advanced features, configuration patterns, and best practices.

## Folder Structure

### Root Files

- **`Program.cs`** - ⭐ **Main application demonstrating the validation system**
- **`Validation.SampleApp.csproj`** - Project configuration with necessary dependencies

## Application Features

The sample application demonstrates:

1. **System Configuration** - Fluent builder pattern setup
2. **Enhanced Validation** - Named rules and detailed results
3. **Event Handling** - Unified event processing
4. **Async Operations** - Asynchronous validation patterns
5. **Error Handling** - Comprehensive error scenarios
6. **Metrics and Observability** - Built-in monitoring

## Running the Sample

### Prerequisites

- .NET 8.0 or later
- Visual Studio 2022, VS Code, or any .NET-compatible IDE

### Execution

```bash
cd Validation.SampleApp
dotnet run
```

### Expected Output

```
Unified Validation System Sample Application
===========================================

Starting unified validation system demonstration...

1. Testing Enhanced Manual Validator Service
--------------------------------------------
Valid item (metric=100): True
Invalid item (metric=-5): False
Failed rules: PositiveValue
Async validation result: True

2. Testing Unified Event System
-------------------------------
Processing DeleteValidationCompleted for Item {guid} at {timestamp}
  Audit ID: {audit-guid}
  Audit Details: Delete validation successful

Processing SaveValidationCompleted for Item {guid} at {timestamp}
  Audit ID: {audit-guid}

Processing SoftDeleteCompleted for Item {guid} at {timestamp}
  Audit ID: {audit-guid}

Processing ValidationOperationFailed for Item {guid} at {timestamp}

Press any key to exit...
```

## Configuration Examples

### Basic Configuration

The sample shows how to set up a basic validation system:

```csharp
services.AddSetupValidation()
    .AddValidationFlow<Item>(flow => flow
        .EnableSaveValidation()
        .EnableDeleteValidation()
        .EnableSoftDelete()
        .WithThreshold(x => x.Metric, ThresholdType.GreaterThan, 5)
        .WithValidationTimeout(TimeSpan.FromMinutes(1))
        .EnableAuditing())
    
    .AddRule<Item>("PositiveValue", item => item.Metric > 0)
    .AddRule<Item>("ReasonableRange", item => item.Metric <= 1000)
    
    .ConfigureMetrics(metrics => metrics
        .WithProcessingInterval(TimeSpan.FromSeconds(30))
        .EnableDetailedMetrics(false))
    
    .ConfigureReliability(reliability => reliability
        .WithMaxRetries(2)
        .WithRetryDelay(TimeSpan.FromMilliseconds(500)))
    
    .Build();
```

### Advanced Configuration Features

The sample demonstrates several advanced configuration options:

#### Validation Flow Configuration
- **Save Validation**: Validates entities before saving
- **Delete Validation**: Validates entities before deletion
- **Soft Delete**: Implements soft delete with restoration capabilities
- **Threshold Rules**: Configures threshold-based validation
- **Timeouts**: Sets validation operation timeouts
- **Auditing**: Enables comprehensive audit trails

#### Metrics Configuration
- **Processing Intervals**: Configures how often metrics are processed
- **Detailed Metrics**: Controls the level of metrics detail
- **Custom Collectors**: Shows how to add custom metrics collection

#### Reliability Configuration
- **Retry Policies**: Configures automatic retry on failures
- **Retry Delays**: Sets delays between retry attempts
- **Circuit Breakers**: Implements circuit breaker patterns

## Demonstration Scenarios

### 1. Enhanced Manual Validator Service

```csharp
private static async Task DemonstrateValidation(IEnhancedManualValidatorService validator, ILogger logger)
{
    // Test valid item
    var validItem = new Item(100);
    var validResult = validator.ValidateWithDetails(validItem);
    
    logger.LogInformation("Valid item (metric={Metric}): {IsValid}", 
        validItem.Metric, validResult.IsValid);
    
    // Test invalid item
    var invalidItem = new Item(-5);
    var invalidResult = validator.ValidateWithDetails(invalidItem);
    
    logger.LogInformation("Invalid item (metric={Metric}): {IsValid}", 
        invalidItem.Metric, invalidResult.IsValid);
    
    if (!invalidResult.IsValid)
    {
        logger.LogWarning("Failed rules: {FailedRules}", 
            string.Join(", ", invalidResult.FailedRules));
    }

    // Test async validation
    var asyncResult = await validator.ValidateAsync(validItem);
    logger.LogInformation("Async validation result: {IsValid}", asyncResult.IsValid);
}
```

**Demonstrates:**
- Synchronous validation with detailed results
- Named rule failures tracking
- Asynchronous validation patterns
- Comprehensive logging and error reporting

### 2. Unified Event System

```csharp
private static async Task DemonstrateUnifiedEvents(ILogger logger)
{
    // Create various unified events
    var deleteEvent = new DeleteValidationCompleted(
        Guid.NewGuid(), "Item", true, Guid.NewGuid(), "Delete validation successful");
    
    var saveEvent = new SaveValidationCompleted(
        Guid.NewGuid(), "Item", true, new { Metric = 150 }, Guid.NewGuid());

    var softDeleteEvent = new SoftDeleteCompleted(
        Guid.NewGuid(), "Item", DateTime.UtcNow, "admin", Guid.NewGuid());

    var failureEvent = new ValidationOperationFailed(
        Guid.NewGuid(), "Item", "Save", "Database connection timeout");

    // Process events using unified interfaces
    ProcessValidationEvent(deleteEvent, logger);
    ProcessValidationEvent(saveEvent, logger);
    ProcessValidationEvent(softDeleteEvent, logger);
    ProcessValidationEvent(failureEvent, logger);
}
```

**Demonstrates:**
- Creating different types of validation events
- Unified event processing patterns
- Event interface polymorphism
- Audit trail integration

### 3. Event Processing Patterns

```csharp
private static void ProcessValidationEvent(IValidationEvent validationEvent, ILogger logger)
{
    logger.LogInformation("Processing {EventType} for {EntityType} {EntityId} at {Timestamp}",
        validationEvent.GetType().Name,
        validationEvent.EntityType,
        validationEvent.EntityId,
        validationEvent.Timestamp);

    // Handle auditable events
    if (validationEvent is IAuditableEvent auditableEvent 
        && auditableEvent.AuditId.HasValue)
    {
        logger.LogInformation("  Audit ID: {AuditId}", auditableEvent.AuditId);
        if (!string.IsNullOrEmpty(auditableEvent.AuditDetails))
        {
            logger.LogInformation("  Audit Details: {AuditDetails}", auditableEvent.AuditDetails);
        }
    }

    // Handle retryable events
    if (validationEvent is IRetryableEvent retryableEvent)
    {
        logger.LogInformation("  Attempt Number: {AttemptNumber}", retryableEvent.AttemptNumber);
    }
}
```

**Demonstrates:**
- Polymorphic event handling
- Interface-based event processing
- Audit information extraction
- Retry attempt tracking

## Key Learning Points

### 1. Fluent Configuration API

The sample shows the power of the fluent configuration API:

```csharp
// Chain multiple configuration calls
services.AddSetupValidation()
    .UseEntityFramework<MyDbContext>()           // Storage configuration
    .AddValidationFlow<Item>(...)                // Entity-specific flows
    .AddRule<Item>(...)                          // Validation rules
    .ConfigureMetrics(...)                       // Metrics setup
    .ConfigureReliability(...)                   // Reliability patterns
    .Build();                                    // Complete setup
```

### 2. Named Validation Rules

Rules can be named for better error reporting:

```csharp
.AddRule<Item>("PositiveValue", item => item.Metric > 0)
.AddRule<Item>("ReasonableRange", item => item.Metric <= 1000)

// Results include rule names
if (!result.IsValid)
{
    Console.WriteLine($"Failed rules: {string.Join(", ", result.FailedRules)}");
    // Output: "Failed rules: PositiveValue"
}
```

### 3. Threshold-Based Validation

Configure threshold validations declaratively:

```csharp
.WithThreshold(x => x.Metric, ThresholdType.GreaterThan, 5)
```

Supports various threshold types:
- `GreaterThan`
- `LessThan` 
- `GreaterThanOrEqual`
- `LessThanOrEqual`
- `Equal`
- `NotEqual`

### 4. Event-Driven Architecture

Events enable loose coupling and extensibility:

```csharp
// Events implement unified interfaces
IValidationEvent baseEvent = new SaveValidationCompleted(...);
IAuditableEvent auditEvent = new DeleteValidationCompleted(...);
IRetryableEvent retryEvent = new ValidationOperationFailed(...);

// Process events uniformly
ProcessValidationEvent(baseEvent, logger);
```

### 5. Async/Await Patterns

Full async support throughout:

```csharp
// Async validation
var result = await validator.ValidateAsync(item);

// Async event processing
await DemonstrateUnifiedEvents(logger);

// Async configuration
await host.StartAsync();
```

## Configuration Patterns

### Environment-Specific Configuration

```csharp
var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

var builder = services.AddSetupValidation();

if (environment == "Development")
{
    builder.ConfigureMetrics(m => m.EnableDetailedMetrics(true))
           .ConfigureReliability(r => r.WithMaxRetries(1));
}
else if (environment == "Production")
{
    builder.ConfigureMetrics(m => m.EnableDetailedMetrics(false))
           .ConfigureReliability(r => r.WithMaxRetries(5));
}
```

### Conditional Feature Enablement

```csharp
.AddValidationFlow<Item>(flow =>
{
    var f = flow.EnableSaveValidation();
    
    if (enableSoftDelete)
        f.EnableSoftDelete();
    
    if (enableMetrics)
        f.EnableMetrics();
    
    return f;
})
```

## Error Handling Examples

The sample demonstrates comprehensive error handling:

```csharp
try
{
    var result = await validator.ValidateAsync(item);
    if (!result.IsValid)
    {
        logger.LogWarning("Validation failed: {Errors}", 
            string.Join("; ", result.Errors));
    }
}
catch (ValidationException ex)
{
    logger.LogError(ex, "Validation exception occurred");
}
catch (TimeoutException ex)
{
    logger.LogError(ex, "Validation timed out");
}
```

## Performance Considerations

The sample includes performance-conscious patterns:

```csharp
// Efficient metrics configuration
.ConfigureMetrics(metrics => metrics
    .WithProcessingInterval(TimeSpan.FromSeconds(30))  // Reasonable interval
    .EnableDetailedMetrics(false))                     // Reduce overhead

// Reasonable retry configuration
.ConfigureReliability(reliability => reliability
    .WithMaxRetries(2)                                 // Limit retries
    .WithRetryDelay(TimeSpan.FromMilliseconds(500)))   // Quick retries
```

## Extending the Sample

### Adding Custom Rules

```csharp
// Add custom validation logic
.AddRule<Item>("EvenNumber", item => item.Metric % 2 == 0)
.AddRule<Item>("DivisibleByFive", item => item.Metric % 5 == 0)
```

### Custom Event Handling

```csharp
// Add custom event processing
private static void ProcessCustomEvent(IValidationEvent evt)
{
    switch (evt)
    {
        case SaveValidationCompleted save:
            Console.WriteLine($"Save completed for {save.EntityType}");
            break;
        case DeleteValidationCompleted delete:
            Console.WriteLine($"Delete completed for {delete.EntityType}");
            break;
        default:
            Console.WriteLine($"Unknown event: {evt.GetType().Name}");
            break;
    }
}
```

### Additional Entity Types

```csharp
// Add validation for multiple entity types
.AddValidationFlow<Item>(flow => flow.EnableSaveValidation())
.AddValidationFlow<NannyRecord>(flow => flow.EnableDeleteValidation())
.AddValidationFlow<CustomEntity>(flow => flow.EnableSoftDelete())
```

## Dependencies

The sample application includes:

- **Microsoft.Extensions.Hosting** - Generic host support
- **Microsoft.Extensions.Logging** - Logging infrastructure
- **Validation.Domain** - Core validation logic
- **Validation.Infrastructure** - Infrastructure services
- **ValidationFlow.Messages** - Message contracts

## Best Practices Demonstrated

1. **Dependency Injection**: Proper DI container usage
2. **Async Patterns**: Correct async/await implementation
3. **Error Handling**: Comprehensive exception management
4. **Logging**: Structured logging with correlation
5. **Configuration**: Environment-aware configuration
6. **Resource Management**: Proper disposal and cleanup
7. **Event Processing**: Unified event handling patterns

This sample application serves as both a learning tool and a starting point for implementing the Unified Validation System in your own applications.