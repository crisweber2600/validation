using MassTransit;
using Microsoft.Extensions.Logging;

namespace Validation.Infrastructure.Pipeline;

/// <summary>
/// Handles invalid metrics by logging and optionally publishing a fault message.
/// </summary>
public class DiscardHandler
{
    private readonly ILogger<DiscardHandler> _logger;
    private readonly IPublishEndpoint _publish;

    public DiscardHandler(ILogger<DiscardHandler> logger, IPublishEndpoint publish)
    {
        _logger = logger;
        _publish = publish;
    }

    public Task HandleAsync<T>(Guid entityId, decimal metric, CancellationToken ct = default)
    {
        _logger.LogWarning("Discarding metric {Metric} for entity {Entity}", metric, entityId);
        return Task.CompletedTask;
    }
}
