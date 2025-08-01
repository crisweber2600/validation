using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Validation.Domain.Events;

namespace Validation.Infrastructure.Messaging;

/// <summary>
/// Implementation of the validation event hub for centralized event handling
/// </summary>
public class ValidationEventHub : IValidationEventHub
{
    private readonly ConcurrentQueue<IValidationEvent> _events = new();
    private readonly ILogger<ValidationEventHub> _logger;

    public ValidationEventHub(ILogger<ValidationEventHub> logger)
    {
        _logger = logger;
    }

    public Task PublishAsync<T>(T validationEvent) where T : IValidationEvent
    {
        _events.Enqueue(validationEvent);
        
        _logger.LogDebug(
            "Published validation event: {EventType} for {EntityType} {EntityId}",
            typeof(T).Name,
            validationEvent.EntityType,
            validationEvent.EntityId);

        return Task.CompletedTask;
    }

    public Task<IEnumerable<IValidationEvent>> GetEventsAsync(string entityType, DateTime? since = null)
    {
        var cutoff = since ?? DateTime.MinValue;
        var filteredEvents = _events
            .Where(e => e.EntityType == entityType && e.Timestamp >= cutoff)
            .OrderBy(e => e.Timestamp)
            .AsEnumerable();

        return Task.FromResult(filteredEvents);
    }
}