using Validation.Domain.Events;

namespace Validation.Domain.Entities;

public class Item : BaseEntity
{
    public decimal Metric { get; private set; }

    public Item(decimal metric)
    {
        Metric = metric;
        AddEvent(new SaveRequested(Id));
    }

    public void UpdateMetric(decimal metric)
    {
        Metric = metric;
        AddEvent(new SaveRequested(Id));
    }

    public void Delete()
    {
        AddEvent(new DeleteRequested(Id));
    }
}
