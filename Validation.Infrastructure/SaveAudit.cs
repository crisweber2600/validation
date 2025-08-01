namespace Validation.Infrastructure;

public class SaveAudit
{
    public Guid Id { get; set; }
    public Guid EntityId { get; set; }
    public string? ApplicationName { get; set; }
    public int BatchSize { get; set; } = 1;
    public bool IsValid { get; set; }
    public decimal Metric { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
