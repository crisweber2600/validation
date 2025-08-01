namespace Validation.Domain.Entities;

public class YourEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public decimal Metric { get; set; }
    public bool Validated { get; set; }
}
