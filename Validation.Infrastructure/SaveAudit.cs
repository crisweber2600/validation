using System.ComponentModel.DataAnnotations;

namespace Validation.Infrastructure;

public class SaveAudit
{
    /// <summary>
    /// Primary key as string for better flexibility
    /// </summary>
    [Key]
    public string Id { get; set; } = Guid.NewGuid().ToString();
    
    /// <summary>
    /// Entity ID as string for compatibility with different ID types
    /// </summary>
    [Required]
    public string EntityId { get; set; } = string.Empty;
    
    /// <summary>
    /// Type of the entity being audited
    /// </summary>
    [Required]
    [MaxLength(500)]
    public string EntityType { get; set; } = string.Empty;
    
    /// <summary>
    /// Application name for multi-tenant support
    /// </summary>
    [MaxLength(200)]
    public string? ApplicationName { get; set; }
    
    /// <summary>
    /// Whether the validation passed
    /// </summary>
    public bool IsValid { get; set; }
    
    /// <summary>
    /// Metric value associated with the validation
    /// </summary>
    public decimal Metric { get; set; }
    
    /// <summary>
    /// Timestamp when the audit record was created
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Additional validation details as JSON
    /// </summary>
    public string? ValidationDetails { get; set; }
    
    /// <summary>
    /// Operation type (Save, Update, Delete, etc.)
    /// </summary>
    [MaxLength(100)]
    public string? OperationType { get; set; }
    
    /// <summary>
    /// User or service that triggered the operation
    /// </summary>
    [MaxLength(200)]
    public string? TriggeredBy { get; set; }
    
    /// <summary>
    /// Validation rules that were applied
    /// </summary>
    public string? AppliedRules { get; set; }
    
    /// <summary>
    /// Correlation ID for tracking related operations
    /// </summary>
    [MaxLength(100)]
    public string? CorrelationId { get; set; }
    
    /// <summary>
    /// Property name for property-specific auditing
    /// </summary>
    [MaxLength(200)]
    public string PropertyName { get; set; } = string.Empty;
    
    /// <summary>
    /// Property value for property-specific auditing
    /// </summary>
    public decimal PropertyValue { get; set; }
}
