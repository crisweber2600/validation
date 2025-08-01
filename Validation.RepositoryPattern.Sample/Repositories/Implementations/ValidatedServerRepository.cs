using Microsoft.Extensions.Logging;
using Validation.Domain.Validation;
using Validation.Infrastructure;
using Validation.RepositoryPattern.Sample.Data;
using Validation.RepositoryPattern.Sample.Models;

namespace Validation.RepositoryPattern.Sample.Repositories.Implementations;

/// <summary>
/// Validated Server repository that integrates validation with server-specific operations
/// </summary>
public class ValidatedServerRepository : ValidatedRepository<Server>, IServerRepository
{
    public ValidatedServerRepository(
        SampleDbContext context, 
        IEnhancedManualValidatorService validator,
        ILogger<ValidatedServerRepository> logger) : base(context, validator, logger)
    {
    }

    public async Task<Server?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        return await FirstOrDefaultAsync(s => s.Name == name, cancellationToken);
    }

    public async Task<IEnumerable<Server>> GetActiveServers(CancellationToken cancellationToken = default)
    {
        return await FindAsync(s => s.IsActive, cancellationToken);
    }

    public async Task<IEnumerable<Server>> GetServersByMemoryRangeAsync(decimal minMemory, decimal maxMemory, CancellationToken cancellationToken = default)
    {
        return await FindAsync(s => s.IsActive && s.Memory >= minMemory && s.Memory <= maxMemory, cancellationToken);
    }

    public async Task<decimal> GetAverageMemoryAsync(CancellationToken cancellationToken = default)
    {
        var activeServers = await GetActiveServers(cancellationToken);
        return activeServers.Any() ? activeServers.Average(s => s.Memory) : 0;
    }

    public async Task<Server?> GetServerWithHighestMemoryAsync(CancellationToken cancellationToken = default)
    {
        var activeServers = await GetActiveServers(cancellationToken);
        return activeServers.OrderByDescending(s => s.Memory).FirstOrDefault();
    }

    public async Task<Server?> GetServerWithLowestMemoryAsync(CancellationToken cancellationToken = default)
    {
        var activeServers = await GetActiveServers(cancellationToken);
        return activeServers.OrderBy(s => s.Memory).FirstOrDefault();
    }
}