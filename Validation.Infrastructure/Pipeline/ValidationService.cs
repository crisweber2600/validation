using Validation.Domain.Validation;
using Validation.Infrastructure.Repositories;

namespace Validation.Infrastructure.Pipeline;

/// <summary>
/// Validates a summary value against previously persisted audits using the
/// configured validation plan.
/// </summary>
public class ValidationService
{
    private readonly ISaveAuditRepository _repository;
    private readonly IValidationPlanProvider _planProvider;
    private readonly SummarisationValidator _validator;

    public ValidationService(ISaveAuditRepository repository, IValidationPlanProvider planProvider, SummarisationValidator validator)
    {
        _repository = repository;
        _planProvider = planProvider;
        _validator = validator;
    }

    /// <summary>
    /// Validate the provided summary for <typeparamref name="T"/>.
    /// </summary>
    public async Task<bool> ValidateAsync<T>(Guid entityId, decimal summary, CancellationToken ct = default)
    {
        var last = await _repository.GetLastAsync(entityId, ct);
        var previous = last?.Metric ?? 0m;
        var plan = _planProvider.GetPlan(typeof(T));
        return _validator.Validate(previous, summary, plan);
    }
}
