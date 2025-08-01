using Validation.RepositoryPattern.Sample.Models;

namespace Validation.RepositoryPattern.Sample.Repositories;

/// <summary>
/// Server repository interface for server-specific operations
/// </summary>
public interface IServerRepository : IRepository<Server>
{
    // Basic server operations
    Task<Server?> GetByNameAsync(string name, CancellationToken cancellationToken = default);
    Task<IEnumerable<Server>> GetActiveServers(CancellationToken cancellationToken = default);
    
    // Server-specific operations
    Task<IEnumerable<Server>> GetServersByMemoryRangeAsync(decimal minMemory, decimal maxMemory, CancellationToken cancellationToken = default);
    Task<decimal> GetAverageMemoryAsync(CancellationToken cancellationToken = default);
    Task<Server?> GetServerWithHighestMemoryAsync(CancellationToken cancellationToken = default);
    Task<Server?> GetServerWithLowestMemoryAsync(CancellationToken cancellationToken = default);
}