using MassTransit;
using Microsoft.Extensions.Logging;
using Validation.Domain.Events;

namespace Validation.Infrastructure.Pipeline;

public class DiscardHandler
{
    private readonly ILogger<DiscardHandler> _logger;
    private readonly IPublishEndpoint _publish;

    public DiscardHandler(ILogger<DiscardHandler> logger, IPublishEndpoint publish)
    {
        _logger = logger;
        _publish = publish;
    }

    public virtual async Task HandleAsync<T>(decimal summary, CancellationToken ct)
    {
        _logger.LogWarning("Metrics for {Type} discarded: {Summary}", typeof(T).Name, summary);
        await _publish.Publish(new SaveCommitFault<T>(Guid.Empty, Guid.Empty, "Validation failed"), ct);
    }
}
