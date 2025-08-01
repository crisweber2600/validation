using System.Threading;
using System.Threading.Tasks;

namespace Validation.Infrastructure.Pipeline;

/// <summary>
/// Abstraction over the event publishing mechanism used by the pipeline.
/// </summary>
public interface IEventPublisher
{
    /// <summary>
    /// Publish an event message.
    /// </summary>
    Task PublishAsync<T>(T message, CancellationToken ct = default);
}

/// <summary>
/// No-op publisher used when none is configured.
/// </summary>
public sealed class NullEventPublisher : IEventPublisher
{
    public Task PublishAsync<T>(T message, CancellationToken ct = default) => Task.CompletedTask;
}
