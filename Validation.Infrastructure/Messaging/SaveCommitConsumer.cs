using MassTransit;
using Validation.Domain.Events;
using Validation.Infrastructure.Repositories;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace Validation.Infrastructure.Messaging;

public class SaveCommitConsumer<T> : IConsumer<SaveValidated<T>>
{
    private readonly ISaveAuditRepository _repository;
    private readonly ILogger<SaveCommitConsumer<T>> _logger;
    private readonly ActivitySource _activitySource;

    /// <summary>
    /// Handles <see cref="SaveValidated{T}"/> events by committing the audit entry.
    /// </summary>
    /// <param name="repository">Repository storing audit information.</param>
    /// <param name="logger">Serilog logger for result messages.</param>
    /// <param name="activitySource">Activity source for tracing.</param>
    public SaveCommitConsumer(ISaveAuditRepository repository, ILogger<SaveCommitConsumer<T>> logger, ActivitySource activitySource)
    {
        _repository = repository;
        _logger = logger;
        _activitySource = activitySource;
    }

    public async Task Consume(ConsumeContext<SaveValidated<T>> context)
    {
        using var activity = _activitySource.StartActivity("SaveCommitConsumer.Consume", ActivityKind.Consumer);
        try
        {
            var audit = await _repository.GetAsync(context.Message.AuditId, context.CancellationToken);
            if (audit != null)
            {
                await _repository.UpdateAsync(audit, context.CancellationToken);
                _logger.LogInformation("Committed audit {AuditId} for entity {EntityId}", audit.Id, audit.EntityId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to commit audit {AuditId} for entity {EntityId}", context.Message.AuditId, context.Message.EntityId);
            await context.Publish(new SaveCommitFault<T>(context.Message.EntityId, context.Message.AuditId, ex.Message));
        }
    }
}