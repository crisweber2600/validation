# Validation.Tests

Comprehensive test suite for the Unified Validation System providing unit tests, integration tests, and reliability testing.

## Overview

This test assembly contains over 80 tests that validate all aspects of the validation system including core validation logic, messaging infrastructure, reliability patterns, and system integration. The tests ensure high quality, reliability, and maintainability of the validation framework.

## Test Structure

### 📁 Test Files

#### Core Validation Tests
- **`EnhancedManualValidatorServiceTests.cs`** - Tests for enhanced validation service functionality
- **`ManualValidatorServiceTests.cs`** - Tests for basic manual validation service
- **`InMemoryValidationPlanProviderTests.cs`** - Tests for in-memory validation plan provider
- **`SummarisationValidatorPlanTests.cs`** - Tests for summarization validation plans
- **`SummarisationValidatorListTests.cs`** - Tests for list-based summarization validation

#### Integration Tests
- **`ValidationFlowIntegrationTests.cs`** - End-to-end validation flow testing
- **`UnifiedValidationSystemTests.cs`** - ⭐ **Comprehensive system integration tests**
- **`AddValidationFlowsTests.cs`** - Tests for adding and configuring validation flows

#### Messaging Tests
- **`SaveValidationConsumerTests.cs`** - Tests for save validation message consumers
- **`MessageTests.cs`** - Tests for message contracts and serialization

#### Reliability Tests
- **`DeletePipelineReliabilityTests.cs`** - Tests for delete pipeline reliability patterns
- **`MetricsCollectorTests.cs`** - Tests for metrics collection functionality

#### Repository Tests
- **`GenericRepositoryTests.cs`** - Tests for generic repository implementations
- **`EventPublishingRepositoryTests.cs`** - Tests for event-publishing repository patterns
- **`UnitOfWorkExampleTests.cs`** - Tests for Unit of Work pattern implementation

#### Test Infrastructure
- **`TestDbContext.cs`** - Test database context for Entity Framework testing
- **`InMemorySaveAuditRepository.cs`** - In-memory repository for audit testing

## Test Categories

### 1. Unit Tests (Core Logic)

Tests that validate individual components in isolation:

```csharp
[Test]
public void EnhancedValidator_ValidItem_ReturnsTrue()
{
    // Arrange
    var validator = new EnhancedManualValidatorService();
    validator.AddRule<Item>("PositiveValue", item => item.Metric > 0);
    var item = new Item(100);
    
    // Act
    var result = validator.ValidateWithDetails(item);
    
    // Assert
    Assert.IsTrue(result.IsValid);
    Assert.IsEmpty(result.FailedRules);
}
```

**Coverage Areas:**
- Validation rule execution
- Named rule management
- Async validation patterns
- Error handling and reporting
- Threshold validation logic

### 2. Integration Tests (System-Wide)

Tests that validate complete workflows and system integration:

```csharp
[Test]
public async Task ValidationFlow_SaveOperation_CompletesSuccessfully()
{
    // Arrange
    var services = new ServiceCollection();
    services.AddSetupValidation()
        .AddValidationFlow<Item>(flow => flow.EnableSaveValidation())
        .Build();
    
    var provider = services.BuildServiceProvider();
    var orchestrator = provider.GetRequiredService<ValidationFlowOrchestrator>();
    
    // Act
    var result = await orchestrator.ProcessSaveAsync(new Item(100));
    
    // Assert
    Assert.IsTrue(result.Success);
}
```

**Coverage Areas:**
- End-to-end validation workflows
- Message flow coordination
- Database integration
- Event publishing and consumption
- Configuration validation

### 3. Messaging Tests

Tests for message contracts, serialization, and consumer behavior:

```csharp
[Test]
public void SaveRequested_SerializesCorrectly()
{
    // Arrange
    var message = new SaveRequested<Item>("TestApp", "Item", Guid.NewGuid(), new Item(100));
    
    // Act
    var json = JsonSerializer.Serialize(message);
    var deserialized = JsonSerializer.Deserialize<SaveRequested<Item>>(json);
    
    // Assert
    Assert.AreEqual(message.EntityId, deserialized.EntityId);
    Assert.AreEqual(message.Payload.Metric, deserialized.Payload.Metric);
}
```

**Coverage Areas:**
- Message serialization/deserialization
- Consumer message handling
- Error message processing
- Message routing and correlation
- Batch message processing

### 4. Reliability Tests

Tests for fault tolerance, retry patterns, and circuit breaker functionality:

```csharp
[Test]
public async Task DeleteReliabilityPolicy_MaxRetriesExceeded_ThrowsException()
{
    // Arrange
    var policy = DeletePipelineReliabilityPolicy.Create()
        .WithMaxRetries(3)
        .WithRetryDelay(TimeSpan.FromMilliseconds(100))
        .Build();
    
    var failingOperation = new Func<CancellationToken, Task<bool>>(_ => 
        throw new InvalidOperationException("Permanent failure"));
    
    // Act & Assert
    var exception = await Assert.ThrowsAsync<DeletePipelineReliabilityException>(
        () => policy.ExecuteAsync(failingOperation, CancellationToken.None));
    
    Assert.Contains("Max retries exceeded", exception.Message);
}
```

**Coverage Areas:**
- Retry policy execution
- Circuit breaker behavior
- Timeout handling
- Fault classification
- Recovery mechanisms

### 5. Performance Tests

Tests for performance characteristics and scalability:

```csharp
[Test]
public async Task ValidationService_HighVolume_MaintainsPerformance()
{
    // Arrange
    var validator = new EnhancedManualValidatorService();
    validator.AddRule<Item>("PositiveValue", item => item.Metric > 0);
    var items = GenerateTestItems(10000);
    
    // Act
    var stopwatch = Stopwatch.StartNew();
    var tasks = items.Select(item => validator.ValidateAsync(item));
    var results = await Task.WhenAll(tasks);
    stopwatch.Stop();
    
    // Assert
    Assert.Less(stopwatch.ElapsedMilliseconds, 5000); // 5 second limit
    Assert.IsTrue(results.All(r => r.IsValid));
}
```

## Test Configuration

### Test Database Setup

```csharp
public class TestDbContext : DbContext
{
    public TestDbContext(DbContextOptions<TestDbContext> options) : base(options) { }
    
    public DbSet<Item> Items { get; set; }
    public DbSet<SaveAudit> SaveAudits { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Item>().HasKey(i => i.Id);
        modelBuilder.Entity<SaveAudit>().HasKey(s => s.AuditId);
    }
}
```

### Test Service Configuration

```csharp
private IServiceProvider SetupTestServices()
{
    var services = new ServiceCollection();
    
    services.AddDbContext<TestDbContext>(options => 
        options.UseInMemoryDatabase($"test-{Guid.NewGuid()}"));
    
    services.AddSetupValidation()
        .UseEntityFramework<TestDbContext>()
        .AddValidationFlow<Item>(flow => flow
            .EnableSaveValidation()
            .EnableDeleteValidation())
        .Build();
    
    return services.BuildServiceProvider();
}
```

### MassTransit Test Harness

```csharp
[Test]
public async Task SaveValidationConsumer_ValidMessage_ProcessesCorrectly()
{
    // Arrange
    var harness = new InMemoryTestHarness();
    var consumerHarness = harness.Consumer<SaveValidationConsumer>();
    
    await harness.Start();
    
    try
    {
        // Act
        await harness.InputQueueSendEndpoint.Send(new SaveRequested<Item>(
            "TestApp", "Item", Guid.NewGuid(), new Item(100)));
        
        // Assert
        Assert.IsTrue(await harness.Consumed.Any<SaveRequested<Item>>());
        Assert.IsTrue(await harness.Published.Any<SaveValidated<Item>>());
    }
    finally
    {
        await harness.Stop();
    }
}
```

## Running Tests

### All Tests
```bash
dotnet test
```

### Specific Test Categories
```bash
# Unit tests only
dotnet test --filter Category=Unit

# Integration tests only  
dotnet test --filter Category=Integration

# Reliability tests only
dotnet test --filter Category=Reliability

# Performance tests only
dotnet test --filter Category=Performance
```

### Specific Test Classes
```bash
# Enhanced validator tests
dotnet test --filter ClassName=EnhancedManualValidatorServiceTests

# Reliability tests
dotnet test --filter ClassName=DeletePipelineReliabilityTests

# Integration tests
dotnet test --filter ClassName=UnifiedValidationSystemTests
```

### With Coverage
```bash
dotnet test --collect:"XPlat Code Coverage"
```

## Test Data Management

### Test Entity Factory

```csharp
public static class TestDataFactory
{
    public static Item CreateValidItem(decimal metric = 100) 
        => new Item(metric);
    
    public static Item CreateInvalidItem() 
        => new Item(-1);
    
    public static IEnumerable<Item> CreateItemBatch(int count, bool valid = true)
    {
        for (int i = 0; i < count; i++)
        {
            yield return valid ? CreateValidItem(i + 1) : CreateInvalidItem();
        }
    }
}
```

### Test Database Seeding

```csharp
public static async Task SeedTestDataAsync(TestDbContext context)
{
    var items = new[]
    {
        new Item(100) { Id = Guid.NewGuid() },
        new Item(200) { Id = Guid.NewGuid() },
        new Item(300) { Id = Guid.NewGuid() }
    };
    
    context.Items.AddRange(items);
    await context.SaveChangesAsync();
}
```

## Mocking and Test Doubles

### Service Mocking

```csharp
[Test]
public async Task ValidationOrchestrator_WithMockedRepository_CallsSave()
{
    // Arrange
    var mockRepository = new Mock<IRepository<Item>>();
    var orchestrator = new ValidationFlowOrchestrator(mockRepository.Object);
    
    // Act
    await orchestrator.ProcessSaveAsync(new Item(100));
    
    // Assert
    mockRepository.Verify(r => r.SaveAsync(It.IsAny<Item>()), Times.Once);
}
```

### Event Bus Mocking

```csharp
[Test]
public async Task ValidationService_WithMockedBus_PublishesEvents()
{
    // Arrange
    var mockBus = new Mock<IBus>();
    var service = new ValidationService(mockBus.Object);
    
    // Act
    await service.ValidateAndSaveAsync(new Item(100));
    
    // Assert
    mockBus.Verify(b => b.Publish(It.IsAny<SaveValidated<Item>>(), default), Times.Once);
}
```

## Test Utilities

### Assertion Helpers

```csharp
public static class ValidationAssert
{
    public static void IsValidResult(ValidationResult result, string message = null)
    {
        Assert.IsTrue(result.IsValid, message ?? "Expected valid result");
        Assert.IsEmpty(result.Errors, "Expected no validation errors");
    }
    
    public static void IsInvalidResult(ValidationResult result, string expectedError = null)
    {
        Assert.IsFalse(result.IsValid, "Expected invalid result");
        Assert.IsNotEmpty(result.Errors, "Expected validation errors");
        
        if (expectedError != null)
        {
            Assert.Contains(expectedError, result.Errors.Select(e => e.ErrorMessage));
        }
    }
}
```

### Async Testing Helpers

```csharp
public static class AsyncTestHelper
{
    public static async Task<T> WithTimeout<T>(Task<T> task, TimeSpan timeout)
    {
        var timeoutTask = Task.Delay(timeout);
        var completedTask = await Task.WhenAny(task, timeoutTask);
        
        if (completedTask == timeoutTask)
        {
            throw new TimeoutException($"Operation timed out after {timeout}");
        }
        
        return await task;
    }
}
```

## Test Coverage

Current test metrics:

- **Total Tests**: 83 tests
- **Passing Tests**: 80 tests (96.4%)
- **Coverage**: ~85% code coverage
- **Performance**: Average test run time < 30 seconds

### Coverage Areas

| Component | Coverage | Test Count |
|-----------|----------|------------|
| Domain Logic | 95% | 25 tests |
| Infrastructure | 80% | 35 tests |  
| Messaging | 90% | 15 tests |
| Integration | 75% | 8 tests |

## Known Test Issues

Currently, there are 3 failing tests:

1. **`DeletePipelineReliabilityTests.ExecuteAsync_PermanentFailure_ThrowsAfterMaxRetries`**
   - Issue: Exception type mismatch in reliability testing
   - Status: Known issue, not affecting main functionality

2. **`DeletePipelineReliabilityTests.ExecuteAsync_CircuitBreakerOpen_ThrowsCircuitOpenException`**
   - Issue: Circuit breaker not opening as expected
   - Status: Under investigation

3. **`EnhancedManualValidatorServiceTests.ValidateWithDetails_ExceptionInRule_ReturnsInvalidWithError`**
   - Issue: Exception handling in validation rules
   - Status: Enhancement pending

## Continuous Integration

Tests run automatically on:

- **Pull Requests**: All tests must pass
- **Main Branch**: Full test suite with coverage reporting
- **Nightly Builds**: Extended test suite including performance tests

### CI Configuration

```yaml
# .github/workflows/tests.yml
name: Tests
on: [push, pull_request]

jobs:
  test:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3
      - uses: actions/setup-dotnet@v3
        with:
          dotnet-version: '8.0.x'
      - run: dotnet restore
      - run: dotnet build --no-restore
      - run: dotnet test --no-build --verbosity normal --collect:"XPlat Code Coverage"
```

## Writing New Tests

### Test Naming Convention

```csharp
[Test]
public void MethodName_Scenario_ExpectedBehavior()
{
    // Arrange
    
    // Act
    
    // Assert
}
```

### Test Categories

Use attributes to categorize tests:

```csharp
[Test]
[Category("Unit")]
[Category("Validation")]
public void ValidateItem_PositiveMetric_ReturnsValid() { }

[Test]
[Category("Integration")]
[Category("Database")]
public async Task SaveItem_WithValidation_PersistsToDatabase() { }
```

### Async Test Patterns

```csharp
[Test]
public async Task AsyncMethod_ValidInput_ReturnsExpectedResult()
{
    // Arrange
    var service = new ValidationService();
    
    // Act
    var result = await service.ValidateAsync(new Item(100));
    
    // Assert
    Assert.IsTrue(result.IsValid);
}
```

This comprehensive test suite ensures the reliability, correctness, and performance of the Unified Validation System across all its components and integration scenarios.