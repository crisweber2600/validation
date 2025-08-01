using Validation.Domain.Validation;
using Validation.Infrastructure.Repositories;

namespace Validation.Infrastructure.Pipeline;

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

    public async Task<bool> ValidateAsync<T>(decimal newValue, CancellationToken ct)
    {
        var last = await _repository.GetLastAsync(Guid.Empty, ct);
        var plan = _planProvider.GetPlan(typeof(T));
        return _validator.Validate(last?.Metric ?? 0m, newValue, plan);
    }
}
