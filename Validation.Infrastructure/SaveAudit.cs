namespace Validation.Infrastructure;

public class SaveAudit
{
    public Guid   Id              { get; set; }
    public string EntityId        { get; set; } = string.Empty;
    public string ApplicationName { get; set; } = string.Empty;
    public bool   IsValid         { get; set; }
    public decimal Metric         { get; set; }
    public int    BatchSize       { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
