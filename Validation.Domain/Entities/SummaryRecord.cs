using System;

namespace Validation.Domain.Entities;

public class SummaryRecord
{
    public int Id { get; set; }
    public string ProgramName { get; set; } = string.Empty;
    public string Entity { get; set; } = string.Empty;
    public decimal MetricValue { get; set; }
    public DateTime RecordedAt { get; set; } = DateTime.UtcNow;
    public Guid RuntimeId { get; set; }
}
