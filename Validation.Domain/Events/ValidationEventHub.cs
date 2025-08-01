using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace Validation.Domain.Events;

/// <summary>
/// Implementation of validation event hub for centralized event handling
/// </summary>
public class ValidationEventHub : IValidationEventHub
{
    private readonly ILogger<ValidationEventHub> _logger;
    private readonly ConcurrentDictionary<string, List<IValidationEvent>> _eventStore = new();

    public ValidationEventHub(ILogger<ValidationEventHub> logger)
    {
        _logger = logger;
    }

    public async Task PublishAsync<T>(T validationEvent) where T : IValidationEvent
    {
        try
        {
            var key = $"{validationEvent.EntityType}:{validationEvent.EntityId}";
            
            _eventStore.AddOrUpdate(key, 
                new List<IValidationEvent> { validationEvent },
                (k, existing) => 
                {
                    existing.Add(validationEvent);
                    return existing;
                });

            _logger.LogInformation("Published validation event {EventType} for {EntityType} {EntityId}", 
                typeof(T).Name, validationEvent.EntityType, validationEvent.EntityId);

            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error publishing validation event {EventType} for {EntityType} {EntityId}", 
                typeof(T).Name, validationEvent.EntityType, validationEvent.EntityId);
            throw;
        }
    }

    public async Task<IEnumerable<IValidationEvent>> GetEventsAsync(string entityType, DateTime? since = null)
    {
        try
        {
            var result = new List<IValidationEvent>();
            
            foreach (var kvp in _eventStore)
            {
                var events = kvp.Value
                    .Where(e => e.EntityType == entityType)
                    .Where(e => !since.HasValue || e.Timestamp >= since.Value)
                    .OrderBy(e => e.Timestamp);
                    
                result.AddRange(events);
            }

            _logger.LogDebug("Retrieved {Count} events for entity type {EntityType}", 
                result.Count, entityType);

            return await Task.FromResult(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving events for entity type {EntityType}", entityType);
            throw;
        }
    }
}