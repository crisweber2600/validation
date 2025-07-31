namespace Validation.Infrastructure;

public class SaveAudit
{
    public Guid Id { get; set; }
    public Guid EntityId { get; set; }
    public bool IsValid { get; set; }
    public decimal Metric { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
