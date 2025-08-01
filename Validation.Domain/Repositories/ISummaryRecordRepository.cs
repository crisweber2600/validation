using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Validation.Domain.Repositories;

/// <summary>
/// Generic repository interface for summary record operations
/// </summary>
public interface ISummaryRecordRepository<T>
{
    Task<T?> GetAsync(string id);
    Task<IEnumerable<T>> GetAllAsync();
    Task<IEnumerable<T>> GetByEntityTypeAsync(string entityType);
    Task SaveAsync(T record);
    Task DeleteAsync(string id);
    Task<bool> ExistsAsync(string id);
}

/// <summary>
/// Base interface for entities that can be stored in summary repositories
/// </summary>
public interface ISummaryRecord
{
    string Id { get; set; }
    string EntityType { get; set; }
    DateTime CreatedAt { get; set; }
    DateTime? UpdatedAt { get; set; }
}