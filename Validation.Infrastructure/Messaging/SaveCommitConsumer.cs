using MassTransit;
using Validation.Domain.Events;
using Validation.Infrastructure.Repositories;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace Validation.Infrastructure.Messaging;

/// <summary>
/// Finalises a save operation and records success or failure. Serilog logs and
/// OpenTelemetry spans emitted here make it easier to trace persistence issues.
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

    public async Task Consume(ConsumeContext<SaveValidated<T>> context)
    {
        using var activity = _activitySource.StartActivity("Consume SaveValidated");
        try
        {
            var audit = await _repository.GetAsync(context.Message.AuditId, context.CancellationToken);
            if (audit != null)
            {
                await _repository.UpdateAsync(audit, context.CancellationToken);
                _logger.LogInformation("Audit {AuditId} updated", audit.Id);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update audit {AuditId}", context.Message.AuditId);
            await context.Publish(new SaveCommitFault<T>(context.Message.EntityId, context.Message.AuditId, ex.Message));
        }
    }
}