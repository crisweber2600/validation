using System;
using System.Threading;
using System.Threading.Tasks;
using Validation.Domain.Validation;
using Validation.Infrastructure.Repositories;

namespace Validation.Infrastructure.Pipeline;

/// <summary>
/// Uses configured validation plans to determine if a new summary value should be committed.
/// </summary>
public class ValidationService : IValidationService
{
    private readonly SummarisationValidator _validator;
    private readonly IValidationPlanProvider _planProvider;
    private readonly ISaveAuditRepository _repository;

    public ValidationService(SummarisationValidator validator, IValidationPlanProvider planProvider, ISaveAuditRepository repository)
    {
        _validator = validator;
        _planProvider = planProvider;
        _repository = repository;
    }

    /// <inheritdoc />
    public virtual async Task<bool> ValidateAsync(decimal summary, CancellationToken ct)
    {
        var last = await _repository.GetLastAsync(Guid.Empty, ct);
        var plan = _planProvider.GetPlan(typeof(decimal));
        return _validator.Validate(last?.Metric ?? 0m, summary, plan);
    }
}
