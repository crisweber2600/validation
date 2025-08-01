using Validation.Domain.Validation;
using Validation.Infrastructure.Repositories;

namespace Validation.Infrastructure.Pipeline;

/// <summary>
/// Default implementation that checks the new summary against the last persisted value.
/// </summary>
public class ValidationService : IValidationService
{
    private readonly ISaveAuditRepository _repository;
    private readonly IValidationPlanProvider _plans;
    private readonly SummarisationValidator _validator;

    public ValidationService(ISaveAuditRepository repository, IValidationPlanProvider plans, SummarisationValidator validator)
    {
        _repository = repository;
        _plans = plans;
        _validator = validator;
    }

    public async Task<bool> ValidateAsync<T>(Guid entityId, decimal metric, CancellationToken ct = default)
    {
        var last = await _repository.GetLastAsync(entityId, ct);
        var plan = _plans.GetPlan(typeof(T));
        return _validator.Validate(last?.Metric ?? 0m, metric, plan);
    }
}
