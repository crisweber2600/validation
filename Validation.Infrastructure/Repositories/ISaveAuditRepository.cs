namespace Validation.Infrastructure.Repositories;

public interface ISaveAuditRepository : IRepository<SaveAudit>
{
    /// <summary>
    /// Get the last audit record for an entity (supporting string entity IDs)
    /// </summary>
    Task<SaveAudit?> GetLastAsync(string entityId, CancellationToken ct = default);
    
    /// <summary>
    /// Get the last audit record for an entity (backward compatibility)
    /// </summary>
    Task<SaveAudit?> GetLastAsync(Guid entityId, CancellationToken ct = default);
    
    /// <summary>
    /// Get audit records by entity type
    /// </summary>
    Task<IEnumerable<SaveAudit>> GetByEntityTypeAsync(string entityType, CancellationToken ct = default);
    
    /// <summary>
    /// Get audit records by application name
    /// </summary>
    Task<IEnumerable<SaveAudit>> GetByApplicationAsync(string applicationName, CancellationToken ct = default);
    
    /// <summary>
    /// Get audit records within a time range
    /// </summary>
    Task<IEnumerable<SaveAudit>> GetByTimeRangeAsync(DateTime from, DateTime to, CancellationToken ct = default);
    
    /// <summary>
    /// Get audit records by correlation ID
    /// </summary>
    Task<IEnumerable<SaveAudit>> GetByCorrelationIdAsync(string correlationId, CancellationToken ct = default);
}
