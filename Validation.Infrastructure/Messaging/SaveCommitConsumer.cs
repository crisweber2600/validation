using MassTransit;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using Validation.Domain.Events;
using Validation.Infrastructure.Repositories;

namespace Validation.Infrastructure.Messaging;

/// <summary>
/// Commits validated save audits and logs the outcome. Each consume operation
/// is traced using an <see cref="ActivitySource"/> to diagnose commit failures.
/// </summary>
public class SaveCommitConsumer<T> : IConsumer<SaveValidated<T>>
{
    private readonly ISaveAuditRepository _repository;
    private readonly ILogger<SaveCommitConsumer<T>> _logger;
    private readonly ActivitySource _activitySource;

    public SaveCommitConsumer(
        ISaveAuditRepository repository,
        ILogger<SaveCommitConsumer<T>> logger,
        ActivitySource activitySource)
    {
        _repository = repository;
        _logger = logger;
        _activitySource = activitySource;
    }

    public async Task Consume(ConsumeContext<SaveValidated<T>> context)
    {
        using var activity = _activitySource.StartActivity("SaveCommit.Consume");
        try
        {
            var audit = await _repository.GetAsync(context.Message.AuditId, context.CancellationToken);
            if (audit != null)
            {
                await _repository.UpdateAsync(audit, context.CancellationToken);
                _logger.LogInformation("Committed audit {AuditId} for entity {EntityId}", context.Message.AuditId, context.Message.EntityId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to commit audit {AuditId} for entity {EntityId}", context.Message.AuditId, context.Message.EntityId);
            await context.Publish(new SaveCommitFault<T>(context.Message.EntityId, context.Message.AuditId, ex.Message));
        }
    }
}