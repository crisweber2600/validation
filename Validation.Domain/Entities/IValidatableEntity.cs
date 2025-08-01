namespace Validation.Domain.Entities;

public interface IValidatableEntity
{
    decimal Metric { get; }
    bool Validated { get; set; }
}
