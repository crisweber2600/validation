# Unified Validation System

## Overview

This document describes the Unified Validation System that consolidates and harmonizes all validation features from 24 active pull requests. The system provides a cohesive, powerful, and maintainable validation framework with modern APIs and comprehensive functionality.

## Architecture

The Unified Validation System is built on several core principles:

- **Unified Messaging**: Consistent message patterns across all validation flows
- **Builder Pattern**: Fluent API for easy configuration and setup
- **Event-Driven**: Interface-based events for extensible handling
- **Configurable**: Granular control over features and behavior
- **Backward Compatible**: Existing APIs remain functional
- **Performance-Optimized**: Thread-safe and efficient implementations

## Core Components

### 1. Unified Messaging System (`ValidationFlow.Messages`)

Complete message patterns for all validation scenarios:

```csharp
// Save operations
SaveRequested<T>
SaveValidated<T>
SaveCommitRequested<T>
SaveCommitCompleted<T>
SaveCommitFault<T>

// Delete operations
DeleteRequested<T>
DeleteValidated<T>
DeleteRejected<T>
DeleteCommitRequested<T>
DeleteCommitCompleted<T>
DeleteCommitFault<T>
```

### 2. Unified Event System (`Validation.Domain.Events`)

Interface-based events with consistent handling:

```csharp
public interface IValidationEvent
{
    Guid EntityId { get; }
    string EntityType { get; }
    DateTime Timestamp { get; }
}

public interface IAuditableEvent : IValidationEvent
{
    Guid? AuditId { get; }
    string? AuditDetails { get; }
}

public interface IRetryableEvent : IValidationEvent
{
    int AttemptNumber { get; }
}
```

Event implementations:
- `DeleteValidationCompleted`
- `SaveValidationCompleted`
- `ValidationOperationFailed`
- `SoftDeleteRequested`
- `SoftDeleteCompleted`
- `SoftDeleteRestored`

### 3. Fluent Builder Pattern (`SetupValidationBuilder`)

Comprehensive validation system setup with fluent API:

```csharp
services.AddSetupValidation()
    .UseEntityFramework<MyDbContext>()
    .AddValidationFlow<Item>(flow => flow
        .EnableSaveValidation()
        .EnableDeleteValidation()
        .EnableSoftDelete()
        .WithThreshold(x => x.Metric, ThresholdType.GreaterThan, 100)
        .WithValidationTimeout(TimeSpan.FromMinutes(5))
        .EnableAuditing())
    .ConfigureMetrics(metrics => metrics
        .EnableDetailedMetrics()
        .WithProcessingInterval(TimeSpan.FromMinutes(1)))
    .ConfigureReliability(reliability => reliability
        .WithMaxRetries(3)
        .WithRetryDelay(TimeSpan.FromSeconds(1)))
    .Build();
```

### 4. Enhanced Configuration (`ValidationFlowConfig`)

Comprehensive flow configuration with support for:

- Save validation and commit flows
- Delete validation and commit flows
- Soft delete operations
- Configurable timeouts and retry policies
- Granular auditing and metrics control

```csharp
public class ValidationFlowConfig
{
    public bool SaveValidation { get; set; }
    public bool SaveCommit { get; set; }
    public bool DeleteValidation { get; set; }
    public bool DeleteCommit { get; set; }
    public bool SoftDeleteSupport { get; set; }
    public TimeSpan? ValidationTimeout { get; set; }
    public int? MaxRetryAttempts { get; set; }
    public bool EnableAuditing { get; set; }
    public bool EnableMetrics { get; set; }
    // ... and more
}
```

### 5. Enhanced Manual Validator Service

Named rules, async validation, and comprehensive result details:

```csharp
var validator = serviceProvider.GetService<IEnhancedManualValidatorService>();

// Add named rules
validator.AddRule<Item>("PositiveValue", item => item.Metric > 0);
validator.AddRule<Item>("MaxValue", item => item.Metric <= 1000);

// Validate with detailed results
var result = validator.ValidateWithDetails(item);
Console.WriteLine($"Valid: {result.IsValid}");
Console.WriteLine($"Failed Rules: {string.Join(", ", result.FailedRules)}");
Console.WriteLine($"Errors: {string.Join(", ", result.Errors)}");

// Async validation
var asyncResult = await validator.ValidateAsync(item);
```

## Usage Examples

### Basic Setup

```csharp
// Simple validation setup
services.AddValidation(setup => setup
    .AddValidationFlow<Item>(flow => flow.EnableSaveValidation())
    .AddRule<Item>(item => item.Metric > 0));
```

### Advanced Setup

```csharp
// Comprehensive validation system
services.AddSetupValidation()
    .UseEntityFramework<MyDbContext>(options => 
        options.UseInMemoryDatabase("ValidationDb"))
    
    .AddValidationFlow<Item>(flow => flow
        .EnableSaveValidation()
        .EnableSaveCommit()
        .EnableDeleteValidation()
        .EnableDeleteCommit()
        .EnableSoftDelete()
        .WithThreshold(x => x.Metric, ThresholdType.GreaterThan, 100)
        .WithValidationTimeout(TimeSpan.FromMinutes(5))
        .WithMaxRetryAttempts(3))
    
    .AddRule<Item>("PositiveValue", item => item.Metric > 0)
    .AddRule<Item>("MaxValue", item => item.Metric <= 10000)
    
    .ConfigureMetrics(metrics => metrics
        .WithProcessingInterval(TimeSpan.FromMinutes(1))
        .EnableDetailedMetrics())
    
    .ConfigureReliability(reliability => reliability
        .WithMaxRetries(3)
        .WithRetryDelay(TimeSpan.FromSeconds(2))
        .WithCircuitBreaker(threshold: 5, timeout: TimeSpan.FromMinutes(1)))
    
    .ConfigureAuditing(auditing => auditing
        .EnableDetailedAuditing()
        .WithRetentionPeriod(TimeSpan.FromDays(365)))
    
    .Build();
```

### MongoDB Setup

```csharp
// MongoDB-based validation system
services.AddSetupValidation()
    .UseMongoDB("mongodb://localhost:27017", "validation")
    .AddValidationFlow<Item>(flow => flow
        .EnableSaveValidation()
        .EnableSoftDelete())
    .Build();
```

### Event Handling

```csharp
// Process unified events
void ProcessValidationEvent(IValidationEvent validationEvent)
{
    Console.WriteLine($"Processing {validationEvent.EntityType} event for {validationEvent.EntityId}");
    
    if (validationEvent is IAuditableEvent auditableEvent && auditableEvent.AuditId.HasValue)
    {
        Console.WriteLine($"Audit ID: {auditableEvent.AuditId}");
    }
    
    if (validationEvent is IRetryableEvent retryableEvent)
    {
        Console.WriteLine($"Attempt: {retryableEvent.AttemptNumber}");
    }
}
```

## Features Consolidated

The Unified Validation System consolidates the following features from the original 24 pull requests:

### Message & Event Unification (PRs #138-141)
✅ **Completed**: Unified validation messages across all flows, standardized event messaging system, implemented unified event messages for better consistency, added delete validation flows and manual rules.

### Soft Delete Implementation (PRs #132-134)
✅ **Completed**: Comprehensive soft delete functionality, support for soft delete operations across the validation system, consistency across duplicate implementations.

### Validation Flow Configuration (PRs #135-137)
✅ **Completed**: Extended validation flow configuration to support delete and commit flows, support for delete and commit flows via configuration, flexible flow configuration for different validation scenarios.

### SetupValidation Builder Pattern (PRs #118-125)
✅ **Completed**: Fluent SetupValidationBuilder API, SetupValidation support with comprehensive tests, unified configuration approach with AddSetupValidation, builder and registration helpers, fluent API for validation setup.

### Metrics Pipeline Orchestration (PRs #126-131)
✅ **Completed**: Metrics pipeline orchestrator, pipeline orchestrator with worker support, explicit commit consumer registration, metrics collection and processing.

## Migration Guide

### From Legacy API

Existing code continues to work without changes:

```csharp
// Legacy - still works
services.AddValidationInfrastructure();
services.AddValidatorRule<Item>(item => item.Metric > 0);

// New unified approach
services.AddValidation(setup => setup
    .AddValidationFlow<Item>(flow => flow.EnableSaveValidation())
    .AddRule<Item>(item => item.Metric > 0));
```

### Upgrading to Unified Events

```csharp
// Old domain events - still work
public record DeleteRequested(Guid Id);
public record DeleteValidated(Guid EntityId, Guid AuditId, string EntityType);

// New unified events - recommended
var deleteEvent = new DeleteValidationCompleted(
    entityId, "Item", true, auditId, "Validation completed");
```

## Testing

The system includes comprehensive tests:

- `UnifiedValidationSystemTests` - Tests for builder pattern and configuration
- All existing tests continue to pass (74 total tests)
- Integration tests for unified event handling
- Performance and reliability tests

## Performance

The unified system maintains excellent performance:

- Thread-safe implementations
- Efficient message processing
- Configurable retry policies
- Circuit breaker patterns
- Optimized validation pipelines

## Future Enhancements

The unified architecture supports future enhancements:

- Additional threshold types
- Custom validation rules
- Enhanced metrics collection
- Extended soft delete capabilities
- Advanced reliability patterns

## Conclusion

The Unified Validation System successfully consolidates features from 24 active pull requests into a single, coherent, and powerful validation framework. It maintains backward compatibility while providing modern APIs and comprehensive functionality for all validation scenarios.