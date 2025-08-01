using Validation.Domain.Validation;

namespace Validation.Infrastructure.DI;

public class ValidationFlowConfig
{
    public string Type { get; set; } = string.Empty;
    public bool SaveValidation { get; set; }
    public bool SaveCommit { get; set; }
    public string? MetricProperty { get; set; }
    public ThresholdType? ThresholdType { get; set; }
    public decimal? ThresholdValue { get; set; }
}
