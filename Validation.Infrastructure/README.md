# Validation.Infrastructure

The infrastructure layer providing concrete implementations, messaging, persistence, observability, and cross-cutting concerns for the Unified Validation System.

## Overview

This assembly contains all infrastructure-related implementations that support the domain layer. It provides messaging, persistence, metrics collection, reliability patterns, and integration with external systems while maintaining clean separation from domain logic.

## Folder Structure

### 📁 `Auditing/`
Audit trail and compliance functionality.

*Provides comprehensive audit logging for all validation operations with retention policies and detailed tracking.*

### 📁 `DI/`
Dependency injection and service registration.

- **`ServiceCollectionExtensions.cs`** - ⭐ **Core service registration extensions**
- **`ValidationFlowConfig.cs`** - Configuration model for validation flows

### 📁 `Messaging/`
MassTransit-based messaging infrastructure.

- **`DeleteValidationConsumer.cs`** - Consumes delete validation messages
- **`ReliableDeleteValidationConsumer.cs`** - Delete validation with reliability patterns
- **`SaveCommitConsumer.cs`** - Consumes save commit messages
- **`SaveRequestedConsumer.cs`** - Consumes save requested messages
- **`SaveValidationConsumer.cs`** - Consumes save validation messages
- **`ValidationEventHub.cs`** - Central event publishing and subscription hub

### 📁 `Metrics/`
Metrics collection and processing.

- **`MetricsOrchestrator.cs`** - Orchestrates metrics collection and processing workflows

### 📁 `Observability/`
OpenTelemetry and monitoring integration.

*Provides distributed tracing, telemetry collection, and monitoring capabilities for production environments.*

### 📁 `Pipeline/`
Orchestration and workflow management.

- **`IPipelineOrchestrator.cs`** - Interface for pipeline orchestration
- **`MetricsPipelineOrchestrator.cs`** - Orchestrates metrics processing pipelines
- **`SummarisationPipelineOrchestrator.cs`** - Orchestrates summarization workflows
- **`ValidationFlowOrchestrator.cs`** - ⭐ **Main validation workflow orchestrator**

### 📁 `Reliability/`
Fault tolerance and resilience patterns.

- **`DeletePipelineReliabilityPolicy.cs`** - Reliability patterns for delete operations
- **`MassTransitReliability.cs`** - MassTransit-specific reliability implementations

### 📁 `Repositories/`
Data access implementations.

*Contains concrete repository implementations for Entity Framework, MongoDB, and other storage providers.*

### 📁 `Setup/`
System configuration and builder patterns.

- **`SetupValidationBuilder.cs`** - ⭐ **Main fluent configuration builder**
- **`ValidationSetupService.cs`** - Service for validation system initialization

### Root Files

- **`EnhancedManualValidatorService.cs`** - Enhanced validation service implementation
- **`ManualValidatorService.cs`** - Basic manual validation service
- **`SaveAudit.cs`** - Audit entity for save operations
- **`UnitOfWork.cs`** - Unit of Work pattern implementation

## Key Components

### SetupValidationBuilder

The main entry point for configuring the validation system with fluent API:

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
        .WithProcessingInterval(TimeSpan.FromMinutes(1))
        .EnableDetailedMetrics())
    .ConfigureReliability(reliability => reliability
        .WithMaxRetries(3)
        .WithRetryDelay(TimeSpan.FromSeconds(2)))
    .Build();
```

### ValidationFlowOrchestrator

Coordinates validation workflows and manages the flow of validation operations:

- Handles save validation workflows
- Manages delete validation workflows  
- Coordinates soft delete operations
- Integrates with messaging infrastructure
- Manages timeouts and reliability

### EnhancedManualValidatorService

Advanced validation service with comprehensive features:

```csharp
public interface IEnhancedManualValidatorService : IManualValidatorService
{
    ValidationResult ValidateWithDetails<T>(T instance);
    Task<ValidationResult> ValidateAsync<T>(T instance);
    void AddRule<T>(string name, Func<T, bool> rule);
    void RemoveRule<T>(string name);
    IEnumerable<string> GetRuleNames<T>();
}
```

Features:
- Named validation rules
- Detailed validation results
- Async validation support
- Rule management (add/remove)
- Error tracking and reporting

## Messaging Architecture

### Message Flow

```
Client Request → ValidationFlowOrchestrator → MassTransit → Consumers → Database → Events
```

### Message Types

#### Save Operations
1. **SaveRequested** → Triggers validation workflow
2. **SaveValidated** → Validation completed, ready for commit
3. **SaveCommitRequested** → Commit operation requested
4. **SaveCommitCompleted** → Commit operation finished
5. **SaveCommitFault** → Commit operation failed

#### Delete Operations
1. **DeleteRequested** → Triggers delete validation
2. **DeleteValidated** → Delete validation passed
3. **DeleteRejected** → Delete validation failed
4. **DeleteCommitRequested** → Delete commit requested
5. **DeleteCommitCompleted** → Delete commit finished

### Consumer Pattern

```csharp
public class SaveRequestedConsumer : IConsumer<SaveRequested>
{
    public async Task Consume(ConsumeContext<SaveRequested> context)
    {
        var message = context.Message;
        
        // Perform validation logic
        var validationResult = await _validator.ValidateAsync(message.Entity);
        
        if (validationResult.IsValid)
        {
            await context.Publish(new SaveValidated(message.EntityId, message.AuditId));
        }
        else
        {
            await context.Publish(new ValidationOperationFailed(
                message.EntityId, "Save", validationResult.ErrorMessage));
        }
    }
}
```

## Reliability Patterns

### Circuit Breaker
Prevents cascade failures by temporarily stopping operations to failing services:

```csharp
var policy = DeletePipelineReliabilityPolicy.Create()
    .WithCircuitBreaker(
        threshold: 5,           // Open after 5 failures
        timeout: TimeSpan.FromMinutes(1))  // Stay open for 1 minute
    .Build();
```

### Retry Policy
Automatically retries failed operations with configurable backoff:

```csharp
.ConfigureReliability(reliability => reliability
    .WithMaxRetries(3)
    .WithRetryDelay(TimeSpan.FromSeconds(2))
    .WithExponentialBackoff())
```

### Timeout Management
Ensures operations don't run indefinitely:

```csharp
.WithValidationTimeout(TimeSpan.FromMinutes(5))
.WithOperationTimeout(TimeSpan.FromMinutes(10))
```

## Metrics and Observability

### Metrics Collection

The system collects comprehensive metrics:

- **Validation Performance**: Execution times, success rates
- **Message Processing**: Throughput, latency, error rates  
- **Resource Usage**: Memory, CPU, database connections
- **Business Metrics**: Validation rule hit rates, entity counts

```csharp
.ConfigureMetrics(metrics => metrics
    .WithProcessingInterval(TimeSpan.FromMinutes(1))
    .EnableDetailedMetrics(true)
    .WithCustomCollector<MyMetricsCollector>()
    .WithRetentionPeriod(TimeSpan.FromDays(30)))
```

### OpenTelemetry Integration

Full distributed tracing and telemetry:

- **Tracing**: End-to-end request tracing
- **Metrics**: Performance counters and business metrics
- **Logging**: Structured logging with correlation
- **Health Checks**: System health monitoring

## Storage Implementations

### Entity Framework Integration

```csharp
.UseEntityFramework<MyDbContext>(options => 
    options.UseSqlServer(connectionString))
```

Features:
- Full EF Core integration
- Transaction management
- Migration support
- Multiple database providers
- Connection pooling

### MongoDB Integration

```csharp
.UseMongoDB("mongodb://localhost:27017", "validation")
```

Features:
- Native MongoDB support
- Document-based storage
- GridFS for large documents
- Aggregation pipeline support
- Automatic indexing

## Configuration Options

### ValidationFlowConfig

Comprehensive configuration for validation flows:

```csharp
public class ValidationFlowConfig
{
    public bool SaveValidation { get; set; } = true;
    public bool SaveCommit { get; set; } = true;
    public bool DeleteValidation { get; set; } = true;
    public bool DeleteCommit { get; set; } = true;
    public bool SoftDeleteSupport { get; set; } = false;
    public TimeSpan? ValidationTimeout { get; set; }
    public TimeSpan? OperationTimeout { get; set; }
    public int? MaxRetryAttempts { get; set; } = 3;
    public TimeSpan? RetryDelay { get; set; }
    public bool EnableAuditing { get; set; } = true;
    public bool EnableMetrics { get; set; } = true;
    public bool EnableReliability { get; set; } = true;
    public string? EntityType { get; set; }
    public List<ThresholdConfig> Thresholds { get; set; } = new();
}
```

### Service Registration

The infrastructure provides comprehensive service registration:

```csharp
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddValidationInfrastructure(this IServiceCollection services)
    {
        services.AddMassTransit(x => { /* MassTransit configuration */ });
        services.AddScoped<IEnhancedManualValidatorService, EnhancedManualValidatorService>();
        services.AddScoped<ValidationFlowOrchestrator>();
        services.AddSingleton<MetricsOrchestrator>();
        // ... additional registrations
        return services;
    }
}
```

## Unit of Work Pattern

Manages transactions across multiple repositories:

```csharp
public class UnitOfWork : IUnitOfWork
{
    public async Task<int> SaveChangesAsync()
    {
        // Begin transaction
        using var transaction = await _context.Database.BeginTransactionAsync();
        
        try
        {
            var result = await _context.SaveChangesAsync();
            await transaction.CommitAsync();
            return result;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}
```

## Usage Examples

### Basic Setup

```csharp
services.AddValidationInfrastructure()
    .AddScoped<IMyRepository, MyRepository>()
    .AddDbContext<MyDbContext>(options => 
        options.UseSqlServer(connectionString));
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
        .WithValidationTimeout(TimeSpan.FromMinutes(5))
        .EnableAuditing())
    
    // Configure infrastructure
    .ConfigureMetrics(metrics => metrics
        .WithProcessingInterval(TimeSpan.FromMinutes(1))
        .EnableDetailedMetrics())
    
    .ConfigureReliability(reliability => reliability
        .WithMaxRetries(3)
        .WithCircuitBreaker(5, TimeSpan.FromMinutes(1)))
    
    .ConfigureAuditing(auditing => auditing
        .WithRetentionPeriod(TimeSpan.FromDays(365))
        .EnableDetailedAuditing())
    
    .Build();
```

### Custom Consumers

```csharp
public class CustomValidationConsumer : IConsumer<CustomValidationRequested>
{
    private readonly IValidator<MyEntity> _validator;
    
    public async Task Consume(ConsumeContext<CustomValidationRequested> context)
    {
        var result = await _validator.ValidateAsync(context.Message.Entity);
        
        if (result.IsValid)
        {
            await context.Publish(new CustomValidationCompleted(context.Message.EntityId));
        }
        else
        {
            await context.Publish(new CustomValidationFailed(
                context.Message.EntityId, result.Errors));
        }
    }
}
```

## Dependencies

Key infrastructure dependencies:

- **MassTransit** - Message bus and consumer framework
- **Entity Framework Core** - ORM and database access
- **MongoDB.Driver** - MongoDB client and operations
- **OpenTelemetry** - Observability and telemetry
- **Microsoft.Extensions.***- Configuration, DI, logging, hosting

## Performance Considerations

1. **Message Processing**: Parallel consumer processing with backpressure
2. **Database Access**: Connection pooling and query optimization
3. **Memory Management**: Efficient object lifecycle management  
4. **Caching**: Strategic caching of validation rules and configurations
5. **Async/Await**: Proper async patterns throughout

## Testing

Infrastructure components are thoroughly tested:

```bash
# Run infrastructure tests
dotnet test --filter "FullyQualifiedName~Validation.Infrastructure"
```

Test categories:
- **Integration Tests**: Full system integration with real databases
- **Consumer Tests**: Message consumer behavior and error handling
- **Reliability Tests**: Circuit breaker and retry functionality
- **Performance Tests**: Load testing and throughput validation

## Monitoring and Diagnostics

Built-in monitoring capabilities:

- **Health Checks**: Database connectivity, message bus health
- **Performance Counters**: Custom counters for validation metrics
- **Distributed Tracing**: End-to-end request correlation
- **Error Tracking**: Comprehensive error logging and alerting

This infrastructure layer provides a robust, scalable foundation for enterprise validation scenarios while maintaining clean architecture principles and high testability.