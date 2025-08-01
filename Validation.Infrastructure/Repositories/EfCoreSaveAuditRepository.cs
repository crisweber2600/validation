using Microsoft.EntityFrameworkCore;
using Validation.Infrastructure.Repositories;

namespace Validation.Infrastructure.Repositories;

public class EfCoreSaveAuditRepository : ISaveAuditRepository
{
    private readonly DbContext _context;
    private readonly DbSet<SaveAudit> _set;

    public EfCoreSaveAuditRepository(DbContext context)
    {
        _context = context;
        _set = context.Set<SaveAudit>();
    }

    public async Task AddAsync(SaveAudit entity, CancellationToken ct = default)
    {
        await _set.AddAsync(entity, ct);
        await _context.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var idString = id.ToString();
        var entity = await _set.FindAsync(new object?[] { idString }, ct);
        if (entity != null)
        {
            _set.Remove(entity);
            await _context.SaveChangesAsync(ct);
        }
    }

    public async Task<SaveAudit?> GetAsync(Guid id, CancellationToken ct = default)
    {
        var idString = id.ToString();
        return await _set.FindAsync(new object?[] { idString }, ct);
    }

    public async Task UpdateAsync(SaveAudit entity, CancellationToken ct = default)
    {
        _set.Update(entity);
        await _context.SaveChangesAsync(ct);
    }

    public async Task<SaveAudit?> GetLastAsync(string entityId, CancellationToken ct = default)
    {
        return await _set
            .Where(a => a.EntityId == entityId)
            .OrderByDescending(a => a.Timestamp)
            .FirstOrDefaultAsync(ct);
    }
    
    public async Task<SaveAudit?> GetLastAsync(Guid entityId, CancellationToken ct = default)
    {
        return await GetLastAsync(entityId.ToString(), ct);
    }

    public async Task<IEnumerable<SaveAudit>> GetByEntityTypeAsync(string entityType, CancellationToken ct = default)
    {
        return await _set
            .Where(a => a.EntityType == entityType)
            .OrderByDescending(a => a.Timestamp)
            .ToListAsync(ct);
    }

    public async Task<IEnumerable<SaveAudit>> GetByApplicationAsync(string applicationName, CancellationToken ct = default)
    {
        return await _set
            .Where(a => a.ApplicationName == applicationName)
            .OrderByDescending(a => a.Timestamp)
            .ToListAsync(ct);
    }

    public async Task<IEnumerable<SaveAudit>> GetByTimeRangeAsync(DateTime from, DateTime to, CancellationToken ct = default)
    {
        return await _set
            .Where(a => a.Timestamp >= from && a.Timestamp <= to)
            .OrderByDescending(a => a.Timestamp)
            .ToListAsync(ct);
    }

    public async Task<IEnumerable<SaveAudit>> GetByCorrelationIdAsync(string correlationId, CancellationToken ct = default)
    {
        return await _set
            .Where(a => a.CorrelationId == correlationId)
            .OrderByDescending(a => a.Timestamp)
            .ToListAsync(ct);
    }

    public async Task<SaveAudit?> GetLastAuditAsync(string entityId, string propertyName, CancellationToken ct = default)
    {
        return await _set
            .Where(a => a.EntityId == entityId && a.PropertyName == propertyName)
            .OrderByDescending(a => a.Timestamp)
            .FirstOrDefaultAsync(ct);
    }

    public async Task AddOrUpdateAuditAsync(string entityId, string entityType, string propertyName,
                                          decimal propertyValue, bool isValid,
                                          string? applicationName = null, string? operationType = null,
                                          string? correlationId = null, CancellationToken ct = default)
    {
        var existingAudit = await GetLastAuditAsync(entityId, propertyName, ct);
        
        if (existingAudit != null)
        {
            // Update existing audit
            existingAudit.PropertyValue = propertyValue;
            existingAudit.IsValid = isValid;
            existingAudit.Timestamp = DateTime.UtcNow;
            existingAudit.ApplicationName = applicationName;
            existingAudit.OperationType = operationType;
            existingAudit.CorrelationId = correlationId;
            
            _set.Update(existingAudit);
        }
        else
        {
            // Insert new audit
            var newAudit = new SaveAudit
            {
                Id = Guid.NewGuid().ToString(),
                EntityId = entityId,
                EntityType = entityType,
                PropertyName = propertyName,
                PropertyValue = propertyValue,
                IsValid = isValid,
                ApplicationName = applicationName,
                OperationType = operationType,
                CorrelationId = correlationId,
                Timestamp = DateTime.UtcNow
            };
            
            await _set.AddAsync(newAudit, ct);
        }
        
        await _context.SaveChangesAsync(ct);
    }
}
