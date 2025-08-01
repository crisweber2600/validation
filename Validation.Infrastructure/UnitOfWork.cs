using Validation.Domain.Entities;
using Validation.Domain.Validation;

namespace Validation.Infrastructure;

public class UnitOfWork
{
    private decimal _previousMetric;
    private YourEntity? _pending;
    private readonly SummarisationValidator _validator = new();

    public UnitOfWork(decimal initialMetric = 0m)
    {
        _previousMetric = initialMetric;
    }

    public void Add(YourEntity entity)
    {
        _pending = entity;
    }

    public bool Validated { get; private set; }

    public async Task SaveChangesAsync(ValidationPlan ruleSet)
    {
        if (_pending == null) return;
        Validated = _validator.Validate(_previousMetric, _pending.Metric, ruleSet);
        _pending.Validated = Validated;
        if (Validated)
        {
            _previousMetric = _pending.Metric;
        }
        _pending = null;
        await Task.CompletedTask;
    }
}
