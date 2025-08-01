# Unified Validation System

A comprehensive, modern .NET validation framework that provides fluent configuration, event-driven architecture, and advanced features for enterprise applications.

## Table of Contents

- [Overview](#overview)
- [Features](#features)
- [Quick Start](#quick-start)
- [Architecture](#architecture)
- [Usage Examples](#usage-examples)
- [Configuration](#configuration)
- [Advanced Features](#advanced-features)
- [API Reference](#api-reference)
- [Testing](#testing)
- [Contributing](#contributing)

## Overview

The Unified Validation System is a powerful, flexible validation framework designed to handle complex enterprise validation scenarios. It consolidates multiple validation patterns into a cohesive system with modern APIs, comprehensive functionality, and excellent performance.

### Key Benefits

- 🚀 **Fluent API**: Easy-to-use builder pattern for configuration
- 🔄 **Event-Driven**: Comprehensive event system for extensibility
- 📊 **Metrics & Observability**: Built-in monitoring and metrics collection
- 🛡️ **Reliability**: Circuit breakers, retries, and fault tolerance
- 🗃️ **Storage Agnostic**: Support for Entity Framework and MongoDB
- ⚡ **High Performance**: Thread-safe, efficient implementations
- 🔧 **Configurable**: Granular control over all features

## Features

### Core Validation Features
- Manual validation rules with named rule support
- Async and sync validation capabilities
- Detailed validation results with error tracking
- Threshold-based validation (GreaterThan, LessThan, etc.)
- Soft delete support with restoration capabilities

### Event System
- Unified event interfaces for consistent handling
- Auditable events with full audit trail
- Retryable events with attempt tracking
- Event-driven validation workflows
- Message-based communication patterns

### Infrastructure Features
- **Messaging**: MassTransit-based messaging with consumers
- **Metrics**: Comprehensive metrics collection and processing
- **Reliability**: Circuit breakers, retry policies, and fault tolerance
- **Auditing**: Full audit trail for all validation operations
- **Observability**: OpenTelemetry integration for monitoring

### Storage Support
- **Entity Framework**: Full EF Core integration with in-memory and SQL Server support
- **MongoDB**: Native MongoDB support for document-based storage
- **Unit of Work**: Pattern implementation for transactional operations

## Quick Start

### 1. Installation

Add the packages to your project:

```bash
dotnet add package Validation.Domain
dotnet add package Validation.Infrastructure
```

### 2. Basic Setup

```csharp
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Validation.Domain.Entities;
using Validation.Infrastructure.Setup;

var host = Host.CreateDefaultBuilder()
    .ConfigureServices(services =>
    {
        // Basic validation setup
        services.AddSetupValidation()
            .AddValidationFlow<Item>(flow => flow
                .EnableSaveValidation()
                .EnableDeleteValidation())
            .AddRule<Item>(item => item.Metric > 0)
            .Build();
    })
    .Build();
```

### 3. Use the Validator

```csharp
var validator = host.Services.GetRequiredService<IEnhancedManualValidatorService>();

var item = new Item(100);
var result = validator.ValidateWithDetails(item);

Console.WriteLine($"Valid: {result.IsValid}");
if (!result.IsValid)
{
    Console.WriteLine($"Failed Rules: {string.Join(", ", result.FailedRules)}");
}
```

## Architecture

The system is built on four core principles:

### 1. **Unified Messaging** (`ValidationFlow.Messages`)
Consistent message patterns for all validation flows:
- Save operations: `SaveRequested<T>`, `SaveValidated<T>`, `SaveCommitCompleted<T>`
- Delete operations: `DeleteRequested<T>`, `DeleteValidated<T>`, `DeleteCommitCompleted<T>`

### 2. **Event-Driven Design** (`Validation.Domain.Events`)
Interface-based events with unified handling:
- `IValidationEvent` - Base event interface
- `IAuditableEvent` - Events with audit information
- `IRetryableEvent` - Events that support retries

### 3. **Builder Pattern** (`SetupValidationBuilder`)
Fluent API for comprehensive system configuration:
```csharp
services.AddSetupValidation()
    .UseEntityFramework<MyDbContext>()
    .AddValidationFlow<Item>(flow => flow.EnableSaveValidation())
    .ConfigureMetrics(metrics => metrics.EnableDetailedMetrics())
    .ConfigureReliability(reliability => reliability.WithMaxRetries(3))
    .Build();
```

### 4. **Modular Infrastructure** (`Validation.Infrastructure`)
Comprehensive infrastructure with:
- Messaging and event handling
- Metrics collection and processing
- Reliability patterns and fault tolerance
- Auditing and observability

## Usage Examples

### Basic Validation

```csharp
// Configure validation
services.AddValidation(setup => setup
    .AddValidationFlow<Item>(flow => flow.EnableSaveValidation())
    .AddRule<Item>(item => item.Metric > 0));

// Use validator
var validator = serviceProvider.GetService<IEnhancedManualValidatorService>();
var result = validator.ValidateWithDetails(item);
```

### Advanced Configuration

```csharp
services.AddSetupValidation()
    .UseEntityFramework<MyDbContext>()
    
    // Configure validation flows
    .AddValidationFlow<Item>(flow => flow
        .EnableSaveValidation()
        .EnableDeleteValidation()
        .EnableSoftDelete()
        .WithThreshold(x => x.Metric, ThresholdType.GreaterThan, 100)
        .WithValidationTimeout(TimeSpan.FromMinutes(5))
        .EnableAuditing())
    
    // Add validation rules
    .AddRule<Item>("PositiveValue", item => item.Metric > 0)
    .AddRule<Item>("MaxValue", item => item.Metric <= 1000)
    
    // Configure infrastructure
    .ConfigureMetrics(metrics => metrics
        .WithProcessingInterval(TimeSpan.FromMinutes(1))
        .EnableDetailedMetrics())
    .ConfigureReliability(reliability => reliability
        .WithMaxRetries(3)
        .WithRetryDelay(TimeSpan.FromSeconds(2)))
    
    .Build();
```

### MongoDB Integration

```csharp
services.AddSetupValidation()
    .UseMongoDB("mongodb://localhost:27017", "validation")
    .AddValidationFlow<Item>(flow => flow
        .EnableSaveValidation()
        .EnableSoftDelete())
    .Build();
```

### Event Handling

```csharp
void ProcessValidationEvent(IValidationEvent validationEvent)
{
    Console.WriteLine($"Processing {validationEvent.EntityType} event");
    
    if (validationEvent is IAuditableEvent auditableEvent)
    {
        Console.WriteLine($"Audit ID: {auditableEvent.AuditId}");
    }
    
    if (validationEvent is IRetryableEvent retryableEvent)
    {
        Console.WriteLine($"Attempt: {retryableEvent.AttemptNumber}");
    }
}
```

## Configuration

### ValidationFlowConfig Options

```csharp
public class ValidationFlowConfig
{
    public bool SaveValidation { get; set; }        // Enable save validation
    public bool SaveCommit { get; set; }            // Enable save commit flow
    public bool DeleteValidation { get; set; }      // Enable delete validation
    public bool DeleteCommit { get; set; }          // Enable delete commit flow
    public bool SoftDeleteSupport { get; set; }     // Enable soft delete
    public TimeSpan? ValidationTimeout { get; set; } // Validation timeout
    public int? MaxRetryAttempts { get; set; }      // Maximum retry attempts
    public bool EnableAuditing { get; set; }        // Enable audit trail
    public bool EnableMetrics { get; set; }         // Enable metrics collection
}
```

### Metrics Configuration

```csharp
.ConfigureMetrics(metrics => metrics
    .WithProcessingInterval(TimeSpan.FromMinutes(1))
    .EnableDetailedMetrics(true)
    .WithCustomCollector<MyMetricsCollector>())
```

### Reliability Configuration

```csharp
.ConfigureReliability(reliability => reliability
    .WithMaxRetries(3)
    .WithRetryDelay(TimeSpan.FromSeconds(2))
    .WithCircuitBreaker(threshold: 5, timeout: TimeSpan.FromMinutes(1))
    .WithCustomPolicy<MyReliabilityPolicy>())
```

## Advanced Features

### Named Validation Rules

```csharp
validator.AddRule<Item>("PositiveValue", item => item.Metric > 0);
validator.AddRule<Item>("MaxValue", item => item.Metric <= 1000);

var result = validator.ValidateWithDetails(item);
Console.WriteLine($"Failed Rules: {string.Join(", ", result.FailedRules)}");
```

### Async Validation

```csharp
var result = await validator.ValidateAsync(item);
```

### Soft Delete Operations

```csharp
// Enable soft delete in configuration
.AddValidationFlow<Item>(flow => flow.EnableSoftDelete())

// Events generated:
// - SoftDeleteRequested
// - SoftDeleteCompleted
// - SoftDeleteRestored
```

### Threshold Validation

```csharp
.WithThreshold(x => x.Metric, ThresholdType.GreaterThan, 100)
.WithThreshold(x => x.Value, ThresholdType.LessThan, 1000)
```

### Custom Validation Rules

```csharp
public class CustomRule : IValidationRule<Item>
{
    public bool IsValid(Item item) => item.Metric % 2 == 0;
    public string Name => "EvenNumber";
}

services.AddValidationRule<Item, CustomRule>();
```

## API Reference

### Core Interfaces

#### IEnhancedManualValidatorService
```csharp
public interface IEnhancedManualValidatorService : IManualValidatorService
{
    ValidationResult ValidateWithDetails<T>(T instance);
    Task<ValidationResult> ValidateAsync<T>(T instance);
    void AddRule<T>(string name, Func<T, bool> rule);
}
```

#### IValidationEvent
```csharp
public interface IValidationEvent
{
    Guid EntityId { get; }
    string EntityType { get; }
    DateTime Timestamp { get; }
}
```

#### SetupValidationBuilder
```csharp
public class SetupValidationBuilder
{
    SetupValidationBuilder UseEntityFramework<TContext>();
    SetupValidationBuilder UseMongoDB(string connectionString, string databaseName);
    SetupValidationBuilder AddValidationFlow<T>(Action<ValidationFlowBuilder<T>> configure);
    SetupValidationBuilder AddRule<T>(string name, Func<T, bool> rule);
    SetupValidationBuilder ConfigureMetrics(Action<MetricsBuilder> configure);
    SetupValidationBuilder ConfigureReliability(Action<ReliabilityBuilder> configure);
    void Build();
}
```

### Event Types

- `DeleteValidationCompleted` - Delete validation finished
- `SaveValidationCompleted` - Save validation finished
- `ValidationOperationFailed` - Operation failed
- `SoftDeleteRequested` - Soft delete requested
- `SoftDeleteCompleted` - Soft delete completed
- `SoftDeleteRestored` - Soft delete restored

## Testing

The system includes comprehensive test coverage:

```bash
# Run all tests
dotnet test

# Run specific test categories
dotnet test --filter Category=Unit
dotnet test --filter Category=Integration
```

### Test Structure
- **Unit Tests**: Core validation logic, rules, and utilities
- **Integration Tests**: Full system tests with database and messaging
- **Performance Tests**: Load and performance validation
- **Reliability Tests**: Circuit breaker and retry functionality

### Running Sample Application

```bash
cd Validation.SampleApp
dotnet run
```

The sample application demonstrates:
- Basic validation setup
- Enhanced validator usage
- Event handling
- Unified messaging
- Configuration options

## Project Structure

```
├── Validation.Domain/           # Core domain logic and entities
├── Validation.Infrastructure/   # Infrastructure implementations
├── Validation.Tests/           # Comprehensive test suite
├── ValidationFlow.Messages/    # Message contracts
├── Validation.SampleApp/       # Sample application
└── Validation.Examples.cs      # Usage examples
```

See individual folder README files for detailed information about each component.

## Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Add/update tests
5. Ensure all tests pass
6. Submit a pull request

### Development Requirements

- .NET 8.0 or later
- Entity Framework Core 8.0+
- MongoDB (for MongoDB features)
- Visual Studio 2022 or VS Code

## License

This project is licensed under the MIT License - see the LICENSE file for details.

## Support

For questions, issues, or contributions:
- Create an issue in the GitHub repository
- Check the existing documentation
- Review the sample application for usage examples