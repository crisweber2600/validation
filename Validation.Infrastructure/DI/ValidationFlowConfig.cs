using Validation.Domain.Validation;
using System.Collections.Generic;

namespace Validation.Infrastructure.DI;

public class ValidationFlowConfig
{
    public string Type { get; set; } = string.Empty;
    public bool SaveValidation { get; set; }
    public bool SaveCommit { get; set; }
    public bool DeleteValidation { get; set; } = true;
    public bool DeleteCommit { get; set; } = true;
    public bool SoftDeleteSupport { get; set; } = false;
    public string? MetricProperty { get; set; }
    public ThresholdType? ThresholdType { get; set; }
    public decimal? ThresholdValue { get; set; }
    public TimeSpan? ValidationTimeout { get; set; }
    public int? MaxRetryAttempts { get; set; }
    public bool EnableAuditing { get; set; } = true;
    public bool EnableMetrics { get; set; } = true;
    
    // Advanced rule configuration
    public List<ValidationRuleConfig> ValidationRules { get; set; } = new();
    public bool EnableCircuitBreaker { get; set; } = false;
    public int? CircuitBreakerThreshold { get; set; }
    public TimeSpan? CircuitBreakerTimeout { get; set; }
    public string? Priority { get; set; } // High, Medium, Low
    public Dictionary<string, object> CustomConfiguration { get; set; } = new();
}

/// <summary>
/// Configuration for individual validation rules
/// </summary>
public class ValidationRuleConfig
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool Enabled { get; set; } = true;
    public int Priority { get; set; } = 100;
    public bool IsRequired { get; set; } = false;
    public TimeSpan? Timeout { get; set; }
    public Dictionary<string, object> Parameters { get; set; } = new();
}