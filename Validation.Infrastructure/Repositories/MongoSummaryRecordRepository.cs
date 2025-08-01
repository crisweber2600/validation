using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using Validation.Domain.Repositories;

namespace Validation.Infrastructure.Repositories;

/// <summary>
/// MongoDB implementation of summary record repository
/// </summary>
public class MongoSummaryRecordRepository<T> : ISummaryRecordRepository<T> 
    where T : class, ISummaryRecord
{
    private readonly IMongoCollection<T> _collection;
    private readonly ILogger<MongoSummaryRecordRepository<T>> _logger;

    public MongoSummaryRecordRepository(
        IMongoDatabase database,
        ILogger<MongoSummaryRecordRepository<T>> logger)
    {
        _collection = database.GetCollection<T>(typeof(T).Name + "s");
        _logger = logger;
    }

    public async Task<T?> GetAsync(string id)
    {
        try
        {
            var filter = Builders<T>.Filter.Eq(r => r.Id, id);
            return await _collection.Find(filter).FirstOrDefaultAsync();
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
            return await _collection.Find(_ => true).ToListAsync();
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
            var filter = Builders<T>.Filter.Eq(r => r.EntityType, entityType);
            return await _collection.Find(filter).ToListAsync();
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
            var existingRecord = await GetAsync(record.Id);
            if (existingRecord != null)
            {
                record.UpdatedAt = DateTime.UtcNow;
                var filter = Builders<T>.Filter.Eq(r => r.Id, record.Id);
                await _collection.ReplaceOneAsync(filter, record);
            }
            else
            {
                record.CreatedAt = DateTime.UtcNow;
                await _collection.InsertOneAsync(record);
            }

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
            var filter = Builders<T>.Filter.Eq(r => r.Id, id);
            var result = await _collection.DeleteOneAsync(filter);
            
            if (result.DeletedCount > 0)
            {
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
            var filter = Builders<T>.Filter.Eq(r => r.Id, id);
            var count = await _collection.CountDocumentsAsync(filter);
            return count > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking existence of summary record with ID {Id}", id);
            throw;
        }
    }
}