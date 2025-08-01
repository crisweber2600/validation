using System.Collections.Generic;
using Validation.Domain.Validation;

namespace Validation.Infrastructure.DI;

public class ValidationFlowConfig
{
    public string Type { get; set; } = string.Empty;
    public bool SaveValidation { get; set; }
    public bool SaveCommit { get; set; }
    public bool DeleteValidation { get; set; } = true;
    public bool DeleteCommit { get; set; } = false;
    public bool SoftDeleteSupport { get; set; } = false;
    public string? MetricProperty { get; set; }
    public ThresholdType? ThresholdType { get; set; }
    public decimal? ThresholdValue { get; set; }
    public TimeSpan? ValidationTimeout { get; set; }
    public int? MaxRetryAttempts { get; set; }
    public bool EnableAuditing { get; set; } = true;
    public bool EnableMetrics { get; set; } = true;
    public List<string> ManualRules { get; set; } = new();
}