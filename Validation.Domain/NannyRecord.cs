namespace Validation.Domain;

public class NannyRecord
{
    public Guid Id { get; set; }
    public Guid EntityId { get; set; }
    public decimal LastMetric { get; set; }
    public string ProgramName { get; set; } = string.Empty;
    public string RuntimeIdentifier { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
