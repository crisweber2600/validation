namespace Validation.Infrastructure;

public class SaveAudit
{
    public Guid   Id              { get; set; }
    public string EntityId        { get; set; } = string.Empty;   // now string
    public string ApplicationName { get; set; } = string.Empty;   // NEW
    public bool   IsValid         { get; set; }
    public decimal Metric         { get; set; }
    public int    BatchSize       { get; set; }                   // NEW
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
