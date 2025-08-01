using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Validation.Domain.Repositories;

namespace Validation.Infrastructure.Repositories;

/// <summary>
/// Entity Framework implementation of summary record repository
/// </summary>
public class EfCoreSummaryRecordRepository<T> : ISummaryRecordRepository<T> 
    where T : class, ISummaryRecord
{
    private readonly DbContext _context;
    private readonly DbSet<T> _dbSet;
    private readonly ILogger<EfCoreSummaryRecordRepository<T>> _logger;

    public EfCoreSummaryRecordRepository(
        DbContext context,
        ILogger<EfCoreSummaryRecordRepository<T>> logger)
    {
        _context = context;
        _dbSet = context.Set<T>();
        _logger = logger;
    }

    public async Task<T?> GetAsync(string id)
    {
        try
        {
            return await _dbSet.FirstOrDefaultAsync(r => r.Id == id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting summary record with ID {Id}", id);
            throw;
        }
    }

    public async Task<IEnumerable<T>> GetAllAsync()
    {
        try
        {
            return await _dbSet.ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all summary records");
            throw;
        }
    }

    public async Task<IEnumerable<T>> GetByEntityTypeAsync(string entityType)
    {
        try
        {
            return await _dbSet.Where(r => r.EntityType == entityType).ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting summary records for entity type {EntityType}", entityType);
            throw;
        }
    }

    public async Task SaveAsync(T record)
    {
        try
        {
            var existing = await _dbSet.FirstOrDefaultAsync(r => r.Id == record.Id);
            if (existing != null)
            {
                record.UpdatedAt = DateTime.UtcNow;
                _context.Entry(existing).CurrentValues.SetValues(record);
            }
            else
            {
                record.CreatedAt = DateTime.UtcNow;
                _dbSet.Add(record);
            }

            await _context.SaveChangesAsync();
            _logger.LogDebug("Saved summary record with ID {Id}", record.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving summary record with ID {Id}", record.Id);
            throw;
        }
    }

    public async Task DeleteAsync(string id)
    {
        try
        {
            var record = await _dbSet.FirstOrDefaultAsync(r => r.Id == id);
            if (record != null)
            {
                _dbSet.Remove(record);
                await _context.SaveChangesAsync();
                _logger.LogDebug("Deleted summary record with ID {Id}", id);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting summary record with ID {Id}", id);
            throw;
        }
    }

    public async Task<bool> ExistsAsync(string id)
    {
        try
        {
            return await _dbSet.AnyAsync(r => r.Id == id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking existence of summary record with ID {Id}", id);
            throw;
        }
    }
}