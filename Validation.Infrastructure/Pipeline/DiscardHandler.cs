using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Validation.Domain.Events;

namespace Validation.Infrastructure.Pipeline;

/// <summary>
/// Handles metrics that fail validation by logging and publishing a fault event.
/// </summary>
public class DiscardHandler
{
    private readonly ILogger<DiscardHandler> _logger;
    private readonly IEventPublisher _publisher;

    public DiscardHandler(ILogger<DiscardHandler> logger, IEventPublisher publisher)
    {
        _logger = logger;
        _publisher = publisher;
    }

    /// <summary>
    /// Process invalid metric summaries.
    /// </summary>
    public virtual async Task HandleAsync(decimal summary, CancellationToken ct)
    {
        _logger.LogWarning("Discarding metric summary {Summary}", summary);
        await _publisher.PublishAsync(new SaveCommitFault<decimal>(Guid.Empty, Guid.Empty, "Validation failed"), ct);
    }
}
