using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Validation.Domain.Entities;
using Validation.Domain.Validation;
using Validation.Infrastructure;
using Validation.Infrastructure.Setup;

class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("Unified Validation System Sample Application");
        Console.WriteLine("===========================================");

        // Create a host with the unified validation system
        var host = Host.CreateDefaultBuilder(args)
            .ConfigureServices((context, services) =>
            {
                // Configure the unified validation system using the fluent builder
                services.AddSetupValidation<Item>(b => b
                        .UseEntityFramework<SampleDbContext>(o => o.UseInMemoryDatabase("sample"))
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
                            .WithRetryDelay(TimeSpan.FromMilliseconds(500))),
                    i => i.Metric)
                    ;
            })
                    .Build();
            })
            .ConfigureLogging(logging =>
            {
                logging.AddConsole();
                logging.SetMinimumLevel(LogLevel.Information);
            })
            .Build();

        // Get the enhanced validator service
        var validator = host.Services.GetRequiredService<IEnhancedManualValidatorService>();
        var logger = host.Services.GetRequiredService<ILogger<Program>>();

        logger.LogInformation("Starting unified validation system demonstration...");

        // Test validation with different scenarios
        await DemonstrateValidation(validator, logger);
        await DemonstrateUnifiedEvents(logger);

        Console.WriteLine("\nPress any key to exit...");
        Console.ReadKey();
    }

    private static async Task DemonstrateValidation(IEnhancedManualValidatorService validator, ILogger logger)
    {
        Console.WriteLine("\n1. Testing Enhanced Manual Validator Service");
        Console.WriteLine("--------------------------------------------");

        // Test valid item
        var validItem = new Item(100);
        var validResult = validator.ValidateWithDetails(validItem);
        
        logger.LogInformation("Valid item (metric={Metric}): {IsValid}", 
            validItem.Metric, validResult.IsValid);
        
        if (!validResult.IsValid)
        {
            logger.LogWarning("Failed rules: {FailedRules}", 
                string.Join(", ", validResult.FailedRules));
        }

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

    private static async Task DemonstrateUnifiedEvents(ILogger logger)
    {
        Console.WriteLine("\n2. Testing Unified Event System");
        Console.WriteLine("-------------------------------");

        // Create various unified events
        var deleteEvent = new Validation.Domain.Events.DeleteValidationCompleted(
            Guid.NewGuid(), "Item", true, Guid.NewGuid(), "Delete validation successful");
        
        var saveEvent = new Validation.Domain.Events.SaveValidationCompleted(
            Guid.NewGuid(), "Item", true, new { Metric = 150 }, Guid.NewGuid());

        var softDeleteEvent = new Validation.Domain.Events.SoftDeleteCompleted(
            Guid.NewGuid(), "Item", DateTime.UtcNow, "admin", Guid.NewGuid());

        var failureEvent = new Validation.Domain.Events.ValidationOperationFailed(
            Guid.NewGuid(), "Item", "Save", "Database connection timeout");

        // Process events using unified interfaces
        ProcessValidationEvent(deleteEvent, logger);
        ProcessValidationEvent(saveEvent, logger);
        ProcessValidationEvent(softDeleteEvent, logger);
        ProcessValidationEvent(failureEvent, logger);

        await Task.CompletedTask;
    }

    private static void ProcessValidationEvent(
        Validation.Domain.Events.IValidationEvent validationEvent,
        ILogger logger)
    {
        logger.LogInformation("Processing {EventType} for {EntityType} {EntityId} at {Timestamp}",
            validationEvent.GetType().Name,
            validationEvent.EntityType,
            validationEvent.EntityId,
            validationEvent.Timestamp);

        // Handle auditable events
        if (validationEvent is Validation.Domain.Events.IAuditableEvent auditableEvent 
            && auditableEvent.AuditId.HasValue)
        {
            logger.LogInformation("  Audit ID: {AuditId}", auditableEvent.AuditId);
            if (!string.IsNullOrEmpty(auditableEvent.AuditDetails))
            {
                logger.LogInformation("  Audit Details: {AuditDetails}", auditableEvent.AuditDetails);
            }
        }

        // Handle retryable events
        if (validationEvent is Validation.Domain.Events.IRetryableEvent retryableEvent)
        {
            logger.LogInformation("  Attempt Number: {AttemptNumber}", retryableEvent.AttemptNumber);
        }
    }
}

public class SampleDbContext : DbContext
{
    public SampleDbContext(DbContextOptions<SampleDbContext> options) : base(options) { }

    public DbSet<Item> Items => Set<Item>();
    public DbSet<SaveAudit> SaveAudits => Set<SaveAudit>();
}