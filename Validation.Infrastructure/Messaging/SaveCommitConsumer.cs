using System;
using MassTransit;
using Validation.Domain.Events;
using Validation.Infrastructure.Repositories;

namespace Validation.Infrastructure.Messaging;

public class SaveCommitConsumer : IConsumer<SaveValidated>
{
    private readonly ISaveAuditRepository _repository;

    public SaveCommitConsumer(ISaveAuditRepository repository)
    {
        _repository = repository;
    }

    public async Task Consume(ConsumeContext<SaveValidated> context)
    {
        var audit = new SaveAudit
        {
            Id = Guid.NewGuid(),
            EntityId = context.Message.Id,
            IsValid = context.Message.Validated,
            Metric = 0m
        };

        await _repository.AddAsync(audit, context.CancellationToken);
    }
}