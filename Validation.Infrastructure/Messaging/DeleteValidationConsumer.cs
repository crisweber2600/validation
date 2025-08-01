using System;
using MassTransit;
using Validation.Domain.Events;
using Validation.Domain.Validation;
using Validation.Infrastructure.Repositories;
using Validation.Infrastructure;

namespace Validation.Infrastructure.Messaging;

/// <summary>
/// Performs validation when an entity delete is requested. A new audit record
/// is written and a <see cref="DeleteValidated{T}"/> event published so that the
/// commit stage can remove the record reliably.
/// </summary>
public class DeleteValidationConsumer<T> : IConsumer<DeleteRequested>
{
    private readonly IValidationPlanProvider _planProvider;
    private readonly ISaveAuditRepository _repository;
    private readonly SummarisationValidator _validator;

    public DeleteValidationConsumer(IValidationPlanProvider planProvider, ISaveAuditRepository repository, SummarisationValidator validator)
    {
        _planProvider = planProvider;
        _repository = repository;
        _validator = validator;
    }

    public async Task Consume(ConsumeContext<DeleteRequested> context)
    {
        var rules = _planProvider.GetRules<T>();
        // execute manual rules with zero metrics since delete; actual logic omitted
        var isValid = _validator.Validate(0, 0, rules);

        var audit = new SaveAudit
        {
            Id = Guid.NewGuid(),
            EntityId = context.Message.Id,
            IsValid = isValid,
            Metric = 0
        };

        await _repository.AddAsync(audit, context.CancellationToken);
        await context.Publish(new DeleteValidated<T>(context.Message.Id, audit.Id));
    }
}