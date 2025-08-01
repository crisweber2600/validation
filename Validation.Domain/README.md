# Validation.Domain

The core domain layer of the Unified Validation System containing entities, validation rules, events, and repositories.

## Overview

This assembly contains the essential domain logic and provides a clean separation between business rules and infrastructure concerns. It defines the core abstractions and implementations that drive the validation system.

## Folder Structure

### 📁 `Entities/`
Core domain entities and their contracts.

- **`IEntityWithEvents.cs`** - Interface for entities that can raise domain events
- **`EntityWithEvents.cs`** - Base class implementation for event-enabled entities
- **`Item.cs`** - Sample entity representing a validatable item with metric properties
- **`NannyRecord.cs`** - Sample entity demonstrating different validation scenarios

### 📁 `Events/`
Domain events and unified event system interfaces.

- **`DeleteRequested.cs`** - Event raised when delete operation is requested
- **`DeleteValidation.cs`** - Events related to delete validation workflows
- **`SaveRequested.cs`** - Event raised when save operation is requested  
- **`SaveRequested.Generic.cs`** - Generic version of save requested event
- **`SaveValidated.cs`** - Event raised when save validation completes
- **`SaveValidated.Generic.cs`** - Generic version of save validated event
- **`SaveCommitFault.cs`** - Event raised when save commit operation fails
- **`UnifiedValidationEvents.cs`** - ⭐ **Central event system with unified interfaces**

### 📁 `Repositories/`
Repository pattern interfaces for data access abstraction.

*Files in this folder define contracts for data access without implementation details.*

### 📁 `Validation/`
Core validation engine and rule definitions.

- **`IManualValidatorService.cs`** - Basic validator service interface
- **`IValidationRule.cs`** - Interface for defining validation rules
- **`IValidationPlanProvider.cs`** - Interface for providing validation plans
- **`IListValidationRule.cs`** - Interface for validating collections
- **`ValidationRule.cs`** - Base implementation of validation rules
- **`ValidationRuleSet.cs`** - Container for multiple validation rules
- **`ValidationPlan.cs`** - Comprehensive validation plan with multiple rules
- **`ValidationStrategy.cs`** - Strategy pattern for different validation approaches
- **`SummarisationValidator.cs`** - Specialized validator for summarization scenarios
- **`InMemoryValidationPlanProvider.cs`** - In-memory implementation of validation plan provider
- **`ThresholdType.cs`** - Enumeration of threshold comparison types (GreaterThan, LessThan, etc.)
- **`PercentChangeRule.cs`** - Rule for validating percentage-based changes
- **`RawDifferenceRule.cs`** - Rule for validating raw difference calculations (marked obsolete)
- **`DuplicateEqualityRule.cs`** - Rule for detecting duplicate values

## Key Interfaces

### IValidationEvent
Central interface for all validation events, enabling unified event handling:

```csharp
public interface IValidationEvent
{
    Guid EntityId { get; }
    string EntityType { get; }
    DateTime Timestamp { get; }
}
```

### IAuditableEvent
Events that include audit information:

```csharp
public interface IAuditableEvent : IValidationEvent
{
    Guid? AuditId { get; }
    string? AuditDetails { get; }
}
```

### IRetryableEvent
Events that support retry mechanisms:

```csharp
public interface IRetryableEvent : IValidationEvent
{
    int AttemptNumber { get; }
}
```

### IValidationRule
Base interface for all validation rules:

```csharp
public interface IValidationRule<in T>
{
    bool IsValid(T item);
    string Name { get; }
}
```

## Core Event Types

### Delete Events
- **`DeleteValidationCompleted`** - Delete validation finished successfully
- **`DeleteValidationRejected`** - Delete validation was rejected with reason

### Save Events
- **`SaveValidationCompleted`** - Save validation finished successfully
- **`ValidationOperationFailed`** - Any validation operation that failed

### Soft Delete Events
- **`SoftDeleteRequested`** - Soft delete operation requested
- **`SoftDeleteCompleted`** - Soft delete operation completed
- **`SoftDeleteRestored`** - Soft delete operation was restored

## Usage Examples

### Creating Custom Entities

```csharp
public class MyEntity : EntityWithEvents
{
    public string Name { get; set; }
    public decimal Value { get; set; }
    
    public void UpdateValue(decimal newValue)
    {
        var oldValue = Value;
        Value = newValue;
        
        // Raise domain event
        AddEvent(new ValueUpdated(Id, oldValue, newValue));
    }
}
```

### Implementing Custom Validation Rules

```csharp
public class PositiveValueRule : IValidationRule<Item>
{
    public string Name => "PositiveValue";
    
    public bool IsValid(Item item)
    {
        return item.Metric > 0;
    }
}
```

### Creating Validation Plans

```csharp
var validationPlan = new ValidationPlan<Item>("ItemValidation")
{
    Rules = new List<IValidationRule<Item>>
    {
        new PositiveValueRule(),
        new ThresholdRule<Item>(x => x.Metric, ThresholdType.LessThan, 1000)
    }
};
```

### Working with Events

```csharp
// Create events
var deleteEvent = new DeleteValidationCompleted(
    entityId: Guid.NewGuid(),
    entityType: "Item", 
    validated: true,
    auditId: Guid.NewGuid(),
    auditDetails: "Validation completed successfully");

// Handle events using unified interface
void ProcessEvent(IValidationEvent evt)
{
    Console.WriteLine($"Processing {evt.EntityType} event for {evt.EntityId}");
    
    if (evt is IAuditableEvent auditableEvent)
    {
        Console.WriteLine($"Audit ID: {auditableEvent.AuditId}");
    }
}
```

## Threshold Types

The `ThresholdType` enumeration supports various comparison operations:

- `GreaterThan` - Value must be greater than threshold
- `LessThan` - Value must be less than threshold  
- `GreaterThanOrEqual` - Value must be greater than or equal to threshold
- `LessThanOrEqual` - Value must be less than or equal to threshold
- `Equal` - Value must equal threshold
- `NotEqual` - Value must not equal threshold

## Validation Strategies

The domain supports multiple validation strategies:

- **Immediate** - Validate immediately upon rule evaluation
- **Deferred** - Defer validation until explicitly triggered
- **Batch** - Validate multiple items as a batch operation
- **Pipeline** - Validate through a series of pipeline stages

## Dependencies

This assembly has minimal dependencies to maintain clean domain boundaries:

- **No infrastructure dependencies** - Pure domain logic
- **No framework dependencies** - Framework-agnostic design
- **Minimal external references** - Only essential .NET types

## Design Principles

1. **Domain-Driven Design** - Clear separation of domain logic
2. **Event Sourcing** - Rich event model for audit and replay
3. **SOLID Principles** - Clean, maintainable, and extensible code
4. **Interface Segregation** - Focused, purpose-specific interfaces
5. **Dependency Inversion** - Abstractions over implementations

## Testing

Domain logic is fully unit testable with high coverage:

```bash
# Run domain-specific tests
dotnet test --filter "FullyQualifiedName~Validation.Domain"
```

The domain layer includes comprehensive tests for:
- Entity behavior and event raising
- Validation rule implementations
- Event interface compliance
- Validation plan execution
- Threshold validation logic

## Extension Points

The domain provides several extension points:

- **Custom Entities** - Implement `IEntityWithEvents` or inherit from `EntityWithEvents`
- **Custom Rules** - Implement `IValidationRule<T>` for entity-specific validation
- **Custom Events** - Implement `IValidationEvent` interfaces for new event types
- **Custom Strategies** - Implement validation strategy interfaces for new approaches
- **Custom Providers** - Implement `IValidationPlanProvider` for custom plan sources

This design ensures the domain remains extensible while maintaining clean boundaries and testability.