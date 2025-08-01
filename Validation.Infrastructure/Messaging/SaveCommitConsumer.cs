using MassTransit;
using Validation.Domain.Events;
using Validation.Infrastructure.Repositories;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace Validation.Infrastructure.Messaging;

/// <summary>
/// Commits validated save audits while logging successes and failures.
/// Each consume operation creates an OpenTelemetry span to aid troubleshooting.
/// </summary>
public class SaveCommitConsumer<T> : IConsumer<SaveValidated<T>>
{
    private readonly ISaveAuditRepository _repository;
    private readonly ILogger<SaveCommitConsumer<T>> _logger;
    private readonly ActivitySource _activitySource;

    public SaveCommitConsumer(ISaveAuditRepository repository, ILogger<SaveCommitConsumer<T>> logger, ActivitySource activitySource)
    {
        _repository = repository;
        _logger = logger;
        _activitySource = activitySource;
    }

    /// <summary>
    /// Applies the commit and records success or failure with logs and traces.
    /// </summary>
    public async Task Consume(ConsumeContext<SaveValidated<T>> context)
    {
        using var activity = _activitySource.StartActivity("SaveCommitConsumer.Consume");
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
            await context.Publish(new SaveCommitFault<T>(context.Message.EntityId, context.Message.AuditId, ex.Message));
            _logger.LogError(ex, "Failed to commit audit {AuditId} for entity {EntityId}", context.Message.AuditId, context.Message.EntityId);
        }
    }
}