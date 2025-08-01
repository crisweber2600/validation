using Validation.Domain.Events;

namespace Validation.Domain.Entities;

public class Measurement : EntityWithEvents, IValidatableEntity
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public decimal Metric { get; private set; }
    public bool Validated { get; set; }

    public Measurement(decimal metric)
    {
        Metric = metric;
        AddEvent(new SaveRequested(Id));
    }

    public void UpdateMetric(decimal metric)
    {
        Metric = metric;
        AddEvent(new SaveRequested(Id));
    }
}
