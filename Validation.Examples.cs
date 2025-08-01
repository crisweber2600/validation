// Example usage of the Unified Validation System

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Validation.Domain.Entities;
using Validation.Domain.Validation;
using Validation.Infrastructure.Setup;

namespace Validation.Examples;

/// <summary>
/// Demonstrates the unified validation system with fluent builder API
/// </summary>
public class UnifiedValidationSystemExample
{
    public static void ConfigureBasicValidation(IServiceCollection services)
    {
        // Basic validation setup with fluent API
        services.AddValidation(setup => setup
            .AddValidationFlow<Item>(flow => flow
                .EnableSaveValidation()
                .EnableDeleteValidation())
            .AddRule<Item>(item => item.Metric > 0));
    }

    public static void ConfigureAdvancedValidation(IServiceCollection services)
    {
        // Advanced validation setup with comprehensive features
        services.AddSetupValidation()
            .UseEntityFramework<MyDbContext>(options => 
                options.UseInMemoryDatabase("ValidationDb"))
            
            // Configure validation flows
            .AddValidationFlow<Item>(flow => flow
                .EnableSaveValidation()
                .EnableSaveCommit()
                .EnableDeleteValidation()
                .EnableDeleteCommit()
                .EnableSoftDelete()
                .WithThreshold(x => x.Metric, ThresholdType.GreaterThan, 100)
                .WithValidationTimeout(TimeSpan.FromMinutes(5))
                .WithMaxRetryAttempts(3)
                .EnableAuditing()
                .EnableMetrics())
            
            .AddValidationFlow<NannyRecord>(flow => flow
                .EnableSaveValidation()
                .EnableDeleteValidation()
                .EnableSoftDelete()
                .WithValidationTimeout(TimeSpan.FromMinutes(2)))
            
            // Add validation rules
            .AddRule<Item>("PositiveValue", item => item.Metric > 0)
            .AddRule<Item>("MaxValue", item => item.Metric <= 10000)
            .AddRule<NannyRecord>(record => !string.IsNullOrEmpty(record.Name))
            
            // Configure system components
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
            
            // Add custom services
            .AddServices(services =>
            {
                services.AddScoped<ICustomValidationService, CustomValidationService>();
            })
            
            .Build();
    }

    public static void ConfigureMinimalValidation(IServiceCollection services)
    {
        // Minimal setup with just core validation
        services.AddSetupValidation()
            .AddValidationFlow<Item>(flow => flow.EnableSaveValidation())
            .DisableMetrics()
            .DisableReliability()
            .DisableAuditing()
            .DisableObservability()
            .Build();
    }

    public static void ConfigureMongoDbValidation(IServiceCollection services)
    {
        // MongoDB-based validation system
        services.AddSetupValidation()
            .UseMongoDB("mongodb://localhost:27017", "validation")
            .AddValidationFlow<Item>(flow => flow
                .EnableSaveValidation()
                .EnableSoftDelete())
            .Build();
    }

    /// <summary>
    /// Demonstrates using the unified event system
    /// </summary>
    public static void HandleUnifiedEvents()
    {
        var deleteEvent = new Validation.Domain.Events.DeleteValidationCompleted(
            Guid.NewGuid(), "Item", true, Guid.NewGuid(), "Validation completed successfully");

        var softDeleteEvent = new Validation.Domain.Events.SoftDeleteCompleted(
            Guid.NewGuid(), "Item", DateTime.UtcNow, "admin", Guid.NewGuid());

        var failureEvent = new Validation.Domain.Events.ValidationOperationFailed(
            Guid.NewGuid(), "Item", "Save", "Database connection failed");

        // Events implement unified interfaces for consistent handling
        ProcessValidationEvent(deleteEvent);
        ProcessValidationEvent(softDeleteEvent);
        ProcessValidationEvent(failureEvent);
    }

    private static void ProcessValidationEvent(Validation.Domain.Events.IValidationEvent validationEvent)
    {
        Console.WriteLine($"Processing {validationEvent.EntityType} event for entity {validationEvent.EntityId} at {validationEvent.Timestamp}");
        
        if (validationEvent is Validation.Domain.Events.IAuditableEvent auditableEvent && auditableEvent.AuditId.HasValue)
        {
            Console.WriteLine($"Audit ID: {auditableEvent.AuditId}");
        }
        
        if (validationEvent is Validation.Domain.Events.IRetryableEvent retryableEvent)
        {
            Console.WriteLine($"Attempt number: {retryableEvent.AttemptNumber}");
        }
    }
}

// Custom example interfaces
public interface ICustomValidationService
{
    Task<bool> ValidateAsync<T>(T entity);
}

public class CustomValidationService : ICustomValidationService
{
    public async Task<bool> ValidateAsync<T>(T entity)
    {
        // Custom validation logic
        await Task.Delay(10);
        return true;
    }
}

public class MyDbContext : Microsoft.EntityFrameworkCore.DbContext
{
    public MyDbContext(Microsoft.EntityFrameworkCore.DbContextOptions<MyDbContext> options) : base(options) { }
    
    public Microsoft.EntityFrameworkCore.DbSet<Item> Items { get; set; }
    public Microsoft.EntityFrameworkCore.DbSet<NannyRecord> NannyRecords { get; set; }
}