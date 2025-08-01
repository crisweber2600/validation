using MassTransit;
using Microsoft.Extensions.Logging;
using Validation.Domain.Events;

namespace Validation.Infrastructure.Pipeline;

/// <summary>
/// Handles metrics that failed validation.
/// </summary>
public class DiscardHandler
{
    private readonly ILogger<DiscardHandler> _logger;
    private readonly IPublishEndpoint _publisher;

    public DiscardHandler(ILogger<DiscardHandler> logger, IPublishEndpoint publisher)
    {
        _logger = logger;
        _publisher = publisher;
    }

    /// <summary>
    /// Log discarded metrics and publish a fault event.
    /// </summary>
    public async Task HandleAsync<T>(IEnumerable<decimal> metrics, CancellationToken ct = default)
    {
        _logger.LogWarning("Discarding {Count} metrics", metrics.Count());
        await _publisher.Publish(new SaveCommitFault<T>(Guid.Empty, Guid.Empty, "Validation failed"), ct);
    }
}
