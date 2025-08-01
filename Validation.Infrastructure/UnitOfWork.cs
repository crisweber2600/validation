using Validation.Domain.Validation;

namespace Validation.Infrastructure;

public class UnitOfWork<T>
{
    private readonly List<T> _items = new();
    private readonly SummarisationValidator _validator = new();
    private decimal? _lastMetric;

    public Task AddAsync(T item)
    {
        _items.Add(item);
        return Task.CompletedTask;
    }

    public Task SaveChangesAsync(EntityValidationRuleSet<T> ruleSet)
    {
        foreach (var item in _items)
        {
            var metric = ruleSet.MetricSelector(item);
            var prev = _lastMetric ?? metric;
            var valid = _validator.Validate(prev, metric, ruleSet.Rules);
            var prop = item!.GetType().GetProperty("Validated");
            if (prop != null && prop.PropertyType == typeof(bool))
                prop.SetValue(item, valid);
            _lastMetric = metric;
        }
        _items.Clear();
        return Task.CompletedTask;
    }
}
