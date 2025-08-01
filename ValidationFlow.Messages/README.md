# ValidationFlow.Messages

Message contracts and communication patterns for the Unified Validation System's distributed messaging infrastructure.

## Overview

This assembly defines all message types, contracts, and communication patterns used throughout the validation system. It provides a comprehensive set of messages for save operations, delete operations, soft delete workflows, and unified validation scenarios.

## Folder Structure

### Root Files

- **`SaveMessages.cs`** - ⭐ **Complete message definitions for save operations**
- **`ValidationFlow.Messages.csproj`** - Project configuration file

## Message Categories

### 1. Save Operation Messages

Complete workflow for save validation and commit operations:

#### Save Validation Flow
```csharp
SaveRequested<T>           // Initial save request with entity data
↓
SaveValidated<T>           // Validation completed successfully
↓  
SaveCommitRequested<T>     // Ready to commit the save operation
↓
SaveCommitCompleted<T>     // Save operation completed successfully
```

#### Error Handling
```csharp
SaveCommitFault<T>         // Save commit operation failed
ValidationOperationFailed   // General validation operation failure
```

### 2. Delete Operation Messages

Complete workflow for delete validation and commit operations:

#### Delete Validation Flow
```csharp
DeleteRequested<T>         // Initial delete request
↓
DeleteValidated<T>         // Delete validation passed
↓
DeleteCommitRequested<T>   // Ready to commit the delete
↓  
DeleteCommitCompleted<T>   // Delete operation completed
```

#### Delete Rejection Flow
```csharp
DeleteRequested<T>         // Initial delete request
↓
DeleteRejected<T>          // Delete validation failed
```

#### Error Handling
```csharp
DeleteCommitFault<T>       // Delete commit operation failed
```

### 3. Soft Delete Messages

Specialized messages for soft delete operations:

```csharp
SoftDeleteRequested<T>     // Soft delete operation requested
SoftDeleteCompleted<T>     // Soft delete operation completed
SoftDeleteRestored<T>      // Soft delete operation restored
```

## Message Structure

### Generic Messages

All messages follow a consistent pattern with generic type support:

```csharp
public record SaveRequested<T>(
    Guid EntityId,
    T Entity,
    Guid? AuditId = null,
    DateTime? RequestedAt = null,
    string? RequestedBy = null
) where T : class;
```

### Non-Generic Messages

Legacy and interoperability messages:

```csharp
public record SaveRequested(
    Guid EntityId,
    object Entity,
    string EntityType,
    Guid? AuditId = null
);
```

## Key Message Types

### SaveRequested<T>
Initiates a save validation workflow.

**Properties:**
- `EntityId` - Unique identifier for the entity
- `Entity` - The entity data to be saved
- `AuditId` - Optional audit trail identifier
- `RequestedAt` - Timestamp of the request
- `RequestedBy` - User or system making the request

**Usage:**
```csharp
var message = new SaveRequested<Item>(
    entityId: Guid.NewGuid(),
    entity: new Item(100),
    auditId: Guid.NewGuid(),
    requestedAt: DateTime.UtcNow,
    requestedBy: "user@example.com"
);

await bus.Publish(message);
```

### SaveValidated<T>
Indicates successful completion of save validation.

**Properties:**
- `EntityId` - Unique identifier for the entity
- `Entity` - The validated entity data
- `ValidationResult` - Detailed validation results
- `AuditId` - Audit trail identifier
- `ValidatedAt` - Timestamp of validation completion

**Usage:**
```csharp
var message = new SaveValidated<Item>(
    entityId: item.Id,
    entity: item,
    validationResult: validationResult,
    auditId: auditId,
    validatedAt: DateTime.UtcNow
);

await context.Publish(message);
```

### DeleteRequested<T>
Initiates a delete validation workflow.

**Properties:**
- `EntityId` - Unique identifier for the entity to delete
- `EntityType` - Type name of the entity
- `AuditId` - Optional audit trail identifier
- `RequestedAt` - Timestamp of the request
- `RequestedBy` - User or system making the request
- `SoftDelete` - Whether this is a soft delete operation

**Usage:**
```csharp
var message = new DeleteRequested<Item>(
    entityId: itemId,
    entityType: "Item",
    auditId: Guid.NewGuid(),
    requestedAt: DateTime.UtcNow,
    requestedBy: "admin@example.com",
    softDelete: true
);

await bus.Publish(message);
```

### DeleteValidated<T>
Indicates successful completion of delete validation.

**Properties:**
- `EntityId` - Unique identifier for the entity
- `EntityType` - Type name of the entity
- `CanDelete` - Whether the entity can be safely deleted
- `ValidationDetails` - Detailed validation information
- `AuditId` - Audit trail identifier

### SaveCommitRequested<T>
Requests the final commit of a validated save operation.

**Properties:**
- `EntityId` - Unique identifier for the entity
- `Entity` - The entity data to commit
- `ValidationContext` - Context from the validation phase
- `AuditId` - Audit trail identifier

### SaveCommitCompleted<T>
Confirms successful completion of the save commit operation.

**Properties:**
- `EntityId` - Unique identifier for the entity
- `Entity` - The committed entity data
- `CommittedAt` - Timestamp of commit completion
- `AuditId` - Audit trail identifier
- `ChangesSummary` - Summary of changes made

## Message Routing

### Topic-Based Routing

Messages are routed using topic-based patterns:

```
validation.save.requested     // SaveRequested messages
validation.save.validated     // SaveValidated messages  
validation.save.committed     // SaveCommitCompleted messages
validation.delete.requested   // DeleteRequested messages
validation.delete.validated   // DeleteValidated messages
validation.soft-delete.*      // All soft delete messages
```

### Fanout Patterns

Some messages support fanout for multiple consumers:

```csharp
// Save validation can trigger multiple downstream processes
SaveValidated<T> → AuditingConsumer
               → MetricsConsumer  
               → NotificationConsumer
               → CommitConsumer
```

## Error Handling Messages

### SaveCommitFault<T>
Indicates a failure during save commit operation.

**Properties:**
- `EntityId` - Unique identifier for the entity
- `ErrorMessage` - Description of the error
- `ErrorCode` - Categorized error code
- `StackTrace` - Technical error details
- `AuditId` - Audit trail identifier
- `RetryAttempt` - Current retry attempt number

### ValidationOperationFailed
General failure message for any validation operation.

**Properties:**
- `EntityId` - Unique identifier for the entity
- `EntityType` - Type name of the entity
- `OperationType` - Type of operation that failed (Save, Delete, etc.)
- `ErrorMessage` - Description of the error
- `ErrorDetails` - Additional error context
- `FailedAt` - Timestamp of failure

## Message Correlation

### Correlation Patterns

All messages support correlation for tracking workflows:

```csharp
// Each message includes correlation information
public record SaveRequested<T>(
    Guid EntityId,                    // Primary correlation ID
    T Entity,
    Guid? AuditId = null,            // Audit correlation
    string? CorrelationId = null,    // Custom correlation
    string? CausationId = null       // Causation tracking
);
```

### Workflow Tracking

Track complete workflows using correlation:

```csharp
var correlationId = Guid.NewGuid().ToString();

// Start workflow
await bus.Publish(new SaveRequested<Item>(itemId, item, correlationId: correlationId));

// All subsequent messages include the same correlation ID
// SaveValidated, SaveCommitRequested, SaveCommitCompleted
```

## Serialization

### JSON Serialization

All messages are designed for JSON serialization:

```csharp
var options = new JsonSerializerOptions
{
    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    WriteIndented = true,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
};

var json = JsonSerializer.Serialize(message, options);
```

### Binary Serialization

Support for high-performance binary serialization:

```csharp
// MessagePack support for high-throughput scenarios
[MessagePackObject]
public record SaveRequested<T>(...) where T : class;
```

## Usage Examples

### Publishing Save Request

```csharp
public class ItemService
{
    private readonly IBus _bus;
    
    public async Task<SaveResult> SaveItemAsync(Item item)
    {
        var auditId = Guid.NewGuid();
        
        var message = new SaveRequested<Item>(
            entityId: item.Id,
            entity: item,
            auditId: auditId,
            requestedAt: DateTime.UtcNow,
            requestedBy: _currentUser.Email
        );
        
        await _bus.Publish(message);
        
        return new SaveResult { AuditId = auditId, Status = SaveStatus.Requested };
    }
}
```

### Consuming Validation Messages

```csharp
public class SaveValidationConsumer : IConsumer<SaveRequested<Item>>
{
    private readonly IValidator<Item> _validator;
    
    public async Task Consume(ConsumeContext<SaveRequested<Item>> context)
    {
        var message = context.Message;
        var validationResult = await _validator.ValidateAsync(message.Entity);
        
        if (validationResult.IsValid)
        {
            await context.Publish(new SaveValidated<Item>(
                entityId: message.EntityId,
                entity: message.Entity,
                validationResult: validationResult,
                auditId: message.AuditId,
                validatedAt: DateTime.UtcNow
            ));
        }
        else
        {
            await context.Publish(new ValidationOperationFailed(
                entityId: message.EntityId,
                entityType: typeof(Item).Name,
                operationType: "Save",
                errorMessage: string.Join("; ", validationResult.Errors)
            ));
        }
    }
}
```

### Request-Response Pattern

```csharp
public class ValidationRequestClient
{
    private readonly IRequestClient<SaveRequested<Item>> _client;
    
    public async Task<SaveResponse> ValidateAndSaveAsync(Item item)
    {
        var request = new SaveRequested<Item>(
            entityId: item.Id,
            entity: item,
            auditId: Guid.NewGuid()
        );
        
        var response = await _client.GetResponse<SaveValidated<Item>, ValidationOperationFailed>(request);
        
        return response.Message switch
        {
            SaveValidated<Item> validated => new SaveResponse { Success = true, Data = validated },
            ValidationOperationFailed failed => new SaveResponse { Success = false, Error = failed.ErrorMessage },
            _ => throw new InvalidOperationException("Unexpected response type")
        };
    }
}
```

## Message Versioning

### Version Strategy

Messages support versioning for backward compatibility:

```csharp
public record SaveRequested<T> : IVersionedMessage
{
    public int Version { get; init; } = 2;  // Current version
    
    // Version 2 properties
    public string? CorrelationId { get; init; }
    public Dictionary<string, object>? Metadata { get; init; }
}
```

### Migration Support

Handle message version migrations:

```csharp
public class SaveRequestedV1ToV2Migrator : IMessageMigrator<SaveRequestedV1, SaveRequested<Item>>
{
    public SaveRequested<Item> Migrate(SaveRequestedV1 source)
    {
        return new SaveRequested<Item>(
            entityId: source.Id,
            entity: source.Data,
            auditId: source.AuditId,
            correlationId: Guid.NewGuid().ToString(),  // New in V2
            metadata: new()  // New in V2
        );
    }
}
```

## Testing

### Message Testing

Test message contracts and serialization:

```csharp
[Test]
public void SaveRequested_SerializesCorrectly()
{
    var item = new Item(100);
    var message = new SaveRequested<Item>(
        entityId: Guid.NewGuid(),
        entity: item,
        auditId: Guid.NewGuid()
    );
    
    var json = JsonSerializer.Serialize(message);
    var deserialized = JsonSerializer.Deserialize<SaveRequested<Item>>(json);
    
    Assert.AreEqual(message.EntityId, deserialized.EntityId);
    Assert.AreEqual(message.Entity.Metric, deserialized.Entity.Metric);
}
```

### Consumer Testing

Test message consumers with test harness:

```csharp
[Test]
public async Task SaveValidationConsumer_ValidEntity_PublishesSaveValidated()
{
    var harness = new InMemoryTestHarness();
    var consumer = harness.Consumer<SaveValidationConsumer>();
    
    await harness.Start();
    
    var item = new Item(100);
    await harness.InputQueueSendEndpoint.Send(new SaveRequested<Item>(
        entityId: item.Id,
        entity: item
    ));
    
    Assert.IsTrue(await harness.Consumed.Any<SaveRequested<Item>>());
    Assert.IsTrue(await harness.Published.Any<SaveValidated<Item>>());
    
    await harness.Stop();
}
```

## Performance Considerations

1. **Message Size**: Keep messages lean, avoid large payloads
2. **Serialization**: Use efficient serialization formats for high throughput
3. **Batching**: Support batch operations for bulk scenarios
4. **Compression**: Enable compression for large messages
5. **Caching**: Cache frequently used message schemas

## Dependencies

Minimal dependencies for clean message contracts:

- **System.Text.Json** - JSON serialization
- **MessagePack** - Binary serialization (optional)
- **MassTransit.Abstractions** - Message bus abstractions

## Best Practices

1. **Immutability**: Use record types for immutable messages
2. **Validation**: Include built-in message validation
3. **Correlation**: Always include correlation identifiers
4. **Versioning**: Plan for message evolution from day one
5. **Documentation**: Document message schemas and workflows
6. **Testing**: Comprehensive testing of all message scenarios

This message system provides a robust foundation for distributed validation workflows while maintaining clean contracts and excellent performance characteristics.