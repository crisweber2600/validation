using Microsoft.EntityFrameworkCore;
using Validation.RepositoryPattern.Sample.Data;
using Validation.RepositoryPattern.Sample.Models;

namespace Validation.RepositoryPattern.Sample.Repositories.Implementations;

/// <summary>
/// Server repository implementation with server-specific operations
/// </summary>
public class ServerRepository : Repository<Server>, IServerRepository
{
    public ServerRepository(SampleDbContext context) : base(context)
    {
    }

    public async Task<Server?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .FirstOrDefaultAsync(s => s.Name == name, cancellationToken);
    }

    public async Task<IEnumerable<Server>> GetActiveServers(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(s => s.IsActive)
            .OrderBy(s => s.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Server>> GetServersByMemoryRangeAsync(decimal minMemory, decimal maxMemory, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(s => s.IsActive && s.Memory >= minMemory && s.Memory <= maxMemory)
            .OrderBy(s => s.Memory)
            .ThenBy(s => s.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<decimal> GetAverageMemoryAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(s => s.IsActive)
            .AverageAsync(s => s.Memory, cancellationToken);
    }

    public async Task<Server?> GetServerWithHighestMemoryAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(s => s.IsActive)
            .OrderByDescending(s => s.Memory)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<Server?> GetServerWithLowestMemoryAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(s => s.IsActive)
            .OrderBy(s => s.Memory)
            .FirstOrDefaultAsync(cancellationToken);
    }
}