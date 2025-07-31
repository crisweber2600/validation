using Validation.Domain.Events;

namespace Validation.Domain.Entities;

public class Item : EntityWithEvents
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public decimal Metric { get; private set; }

    public Item(decimal metric)
    {
        Metric = metric;
        AddEvent(new SaveRequested(Id, metric));
    }

    public void UpdateMetric(decimal metric)
    {
        Metric = metric;
        AddEvent(new SaveRequested(Id, metric));
    }

    public void Delete()
    {
        AddEvent(new DeleteRequested(Id));
    }
}
