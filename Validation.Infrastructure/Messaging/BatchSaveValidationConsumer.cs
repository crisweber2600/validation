using MassTransit;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using Validation.Domain.Events;
using Validation.Domain.Validation;
using Validation.Infrastructure.Repositories;

namespace Validation.Infrastructure.Messaging;

/// <summary>
/// Generic save validation consumer that supports batch processing for performance optimization
/// </summary>
public class BatchSaveValidationConsumer<T> : IConsumer<SaveRequested>
{
    private readonly IValidationPlanProvider _planProvider;
    private readonly ISaveAuditRepository _repository;
    private readonly SummarisationValidator _validator;
    private readonly ILogger<BatchSaveValidationConsumer<T>> _logger;
    private readonly BatchProcessingOptions _options;

    // Thread-safe collection for batching
    private static readonly ConcurrentQueue<BatchItem> _batchQueue = new();
    private static readonly Timer _batchTimer;

    static BatchSaveValidationConsumer()
    {
        // Initialize timer for batch processing
        _batchTimer = new Timer(ProcessBatch, null, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1));
    }

    public BatchSaveValidationConsumer(
        IValidationPlanProvider planProvider, 
        ISaveAuditRepository repository, 
        SummarisationValidator validator,
        ILogger<BatchSaveValidationConsumer<T>> logger,
        BatchProcessingOptions? options = null)
    {
        _planProvider = planProvider;
        _repository = repository;
        _validator = validator;
        _logger = logger;
        _options = options ?? new BatchProcessingOptions();
    }

    public async Task Consume(ConsumeContext<SaveRequested> context)
    {
        if (_options.EnableBatchProcessing)
        {
            // Add to batch queue
            var batchItem = new BatchItem
            {
                Context = context,
                Message = context.Message,
                Timestamp = DateTime.UtcNow
            };
            
            _batchQueue.Enqueue(batchItem);
            _logger.LogDebug("Added item {EntityId} to batch queue", context.Message.Id);
        }
        else
        {
            // Process immediately
            await ProcessSingleItem(context);
        }
    }

    private async Task ProcessSingleItem(ConsumeContext<SaveRequested> context)
    {
        try
        {
            var last = await _repository.GetLastAsync(context.Message.Id.ToString(), context.CancellationToken);
            var metric = new Random().Next(0, 100);
            var rules = _planProvider.GetRules<T>();
            var isValid = _validator.Validate(last?.Metric ?? 0m, metric, rules);

            var audit = new SaveAudit
            {
                Id = Guid.NewGuid().ToString(),
                EntityId = context.Message.Id.ToString(),
                EntityType = typeof(T).Name,
                IsValid = isValid,
                Metric = metric,
                OperationType = "Save",
                CorrelationId = context.CorrelationId?.ToString()
            };

            await _repository.AddAsync(audit, context.CancellationToken);
            await context.Publish(new SaveValidated<T>(context.Message.Id, Guid.Parse(audit.Id)));
            
            _logger.LogDebug("Processed single item {EntityId} with result {IsValid}", context.Message.Id, isValid);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing single item {EntityId}", context.Message.Id);
            throw;
        }
    }

    private static async void ProcessBatch(object? state)
    {
        if (_batchQueue.IsEmpty)
            return;

        var batch = new List<BatchItem>();
        var maxBatchSize = 50; // Configurable
        
        // Dequeue items for batch processing
        while (batch.Count < maxBatchSize && _batchQueue.TryDequeue(out var item))
        {
            batch.Add(item);
        }

        if (batch.Count == 0)
            return;

        try
        {
            await ProcessBatchItems(batch);
        }
        catch (Exception ex)
        {
            // Log error and potentially re-queue items for retry
            Console.WriteLine($"Error processing batch: {ex.Message}");
        }
    }

    private static async Task ProcessBatchItems(List<BatchItem> batch)
    {
        // Group by entity type for efficient processing
        var groupedBatch = batch.GroupBy(item => item.Message.GetType()).ToList();
        
        foreach (var group in groupedBatch)
        {
            await ProcessBatchGroup(group.ToList());
        }
    }

    private static async Task ProcessBatchGroup(List<BatchItem> items)
    {
        // Implement efficient batch processing logic here
        var audits = new List<SaveAudit>();
        var validatedEvents = new List<SaveValidated<T>>();

        foreach (var item in items)
        {
            try
            {
                // Simplified batch processing - in real implementation, 
                // you'd optimize database queries and validation
                var metric = new Random().Next(0, 100);
                var isValid = metric > 50; // Simplified validation

                var audit = new SaveAudit
                {
                    Id = Guid.NewGuid().ToString(),
                    EntityId = item.Message.Id.ToString(),
                    EntityType = typeof(T).Name,
                    IsValid = isValid,
                    Metric = metric,
                    OperationType = "Save",
                    CorrelationId = item.Context.CorrelationId?.ToString()
                };

                audits.Add(audit);
                validatedEvents.Add(new SaveValidated<T>(item.Message.Id, Guid.Parse(audit.Id)));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing batch item {item.Message.Id}: {ex.Message}");
            }
        }

        // Batch save audits and publish events
        // Note: This would need actual repository and event bus instances
        // For now, just simulate the operations
        Console.WriteLine($"Processed batch of {audits.Count} items");
    }

    private class BatchItem
    {
        public ConsumeContext<SaveRequested> Context { get; set; } = null!;
        public SaveRequested Message { get; set; } = null!;
        public DateTime Timestamp { get; set; }
    }
}

/// <summary>
/// Options for batch processing configuration
/// </summary>
public class BatchProcessingOptions
{
    /// <summary>
    /// Enable batch processing instead of individual processing
    /// </summary>
    public bool EnableBatchProcessing { get; set; } = false;
    
    /// <summary>
    /// Maximum number of items in a batch
    /// </summary>
    public int MaxBatchSize { get; set; } = 50;
    
    /// <summary>
    /// Maximum time to wait before processing a partial batch
    /// </summary>
    public TimeSpan BatchTimeout { get; set; } = TimeSpan.FromSeconds(5);
    
    /// <summary>
    /// Number of parallel batches to process concurrently
    /// </summary>
    public int ConcurrentBatches { get; set; } = 1;
}

/// <summary>
/// Generic save validation consumer for reusable validation logic
/// </summary>
public class GenericSaveValidationConsumer : IConsumer<SaveRequested>
{
    private readonly IValidationPlanProvider _planProvider;
    private readonly ISaveAuditRepository _repository;
    private readonly SummarisationValidator _validator;
    private readonly ILogger<GenericSaveValidationConsumer> _logger;

    public GenericSaveValidationConsumer(
        IValidationPlanProvider planProvider,
        ISaveAuditRepository repository,
        SummarisationValidator validator,
        ILogger<GenericSaveValidationConsumer> logger)
    {
        _planProvider = planProvider;
        _repository = repository;
        _validator = validator;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<SaveRequested> context)
    {
        try
        {
            var entityType = context.Message.GetType().GetGenericArguments().FirstOrDefault()?.Name ?? "Unknown";
            
            var last = await _repository.GetLastAsync(context.Message.Id.ToString(), context.CancellationToken);
            var metric = GenerateMetric(context.Message);
            var isValid = await ValidateEntity(context.Message, metric);

            var audit = new SaveAudit
            {
                Id = Guid.NewGuid().ToString(),
                EntityId = context.Message.Id.ToString(),
                EntityType = entityType,
                IsValid = isValid,
                Metric = metric,
                OperationType = "Save",
                CorrelationId = context.CorrelationId?.ToString(),
                Timestamp = DateTime.UtcNow
            };

            await _repository.AddAsync(audit, context.CancellationToken);

            // Publish appropriate event based on entity type
            await PublishValidatedEvent(context, audit);
            
            _logger.LogInformation("Processed save validation for {EntityType} {EntityId} with result {IsValid}", 
                entityType, context.Message.Id, isValid);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing save validation for {EntityId}", context.Message.Id);
            throw;
        }
    }

    private decimal GenerateMetric(SaveRequested message)
    {
        // In a real implementation, this would extract metrics from the entity
        return new Random().Next(0, 100);
    }

    private async Task<bool> ValidateEntity(SaveRequested message, decimal metric)
    {
        // Generic validation logic - can be enhanced based on entity type
        return await Task.FromResult(metric > 0);
    }

    private async Task PublishValidatedEvent(ConsumeContext<SaveRequested> context, SaveAudit audit)
    {
        // For now, publish a generic event - in practice, you'd need type-specific events
        var validatedEvent = new SaveValidationCompleted(
            Guid.Parse(audit.EntityId),
            audit.EntityType,
            audit.IsValid,
            null,
            Guid.Parse(audit.Id),
            audit.ValidationDetails
        );

        await context.Publish(validatedEvent);
    }
}