using Validation.RepositoryPattern.Sample.Models;
using Validation.RepositoryPattern.Sample.Repositories;

namespace Validation.RepositoryPattern.Sample.Services;

/// <summary>
/// Server service interface for business operations
/// </summary>
public interface IServerService
{
    // Basic operations
    Task<Server?> GetServerAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Server?> GetServerByNameAsync(string name, CancellationToken cancellationToken = default);
    Task<IEnumerable<Server>> GetAllServersAsync(CancellationToken cancellationToken = default);
    Task<Server> CreateServerAsync(Server server, CancellationToken cancellationToken = default);
    Task<Server> UpdateServerAsync(Server server, CancellationToken cancellationToken = default);
    Task DeleteServerAsync(Guid id, CancellationToken cancellationToken = default);
    
    // Demo scenario operations
    Task<IEnumerable<Server>> GetServersMemoryAsync(CancellationToken cancellationToken = default);
    Task DemonstrateValidationScenarioAsync(CancellationToken cancellationToken = default);
    
    // Business operations
    Task<Server> UpdateServerMemoryAsync(Guid id, decimal newMemory, CancellationToken cancellationToken = default);
    Task<decimal> GetAverageMemoryAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<Server>> GetServersByMemoryRangeAsync(decimal minMemory, decimal maxMemory, CancellationToken cancellationToken = default);
}

/// <summary>
/// Server service implementation using repository pattern with validation
/// Implements the specific demo scenario described in the requirements
/// </summary>
public class ServerService : IServerService
{
    private readonly IServerRepository _serverRepository;
    private bool _firstCall = true;

    public ServerService(IServerRepository serverRepository)
    {
        _serverRepository = serverRepository ?? throw new ArgumentNullException(nameof(serverRepository));
    }

    public async Task<Server?> GetServerAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _serverRepository.GetByIdAsync(id, cancellationToken);
    }

    public async Task<Server?> GetServerByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        return await _serverRepository.GetByNameAsync(name, cancellationToken);
    }

    public async Task<IEnumerable<Server>> GetAllServersAsync(CancellationToken cancellationToken = default)
    {
        return await _serverRepository.GetActiveServers(cancellationToken);
    }

    public async Task<Server> CreateServerAsync(Server server, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(server);
        
        // Check if name is already taken
        var existingServer = await _serverRepository.GetByNameAsync(server.Name, cancellationToken);
        if (existingServer != null)
        {
            throw new InvalidOperationException($"A server with name '{server.Name}' already exists.");
        }
        
        // Repository will handle validation
        var addedServer = await _serverRepository.AddAsync(server, cancellationToken);
        await _serverRepository.SaveChangesAsync(cancellationToken);
        
        return addedServer;
    }

    public async Task<Server> UpdateServerAsync(Server server, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(server);
        
        var existingServer = await _serverRepository.GetByIdAsync(server.Id, cancellationToken);
        if (existingServer == null)
        {
            throw new InvalidOperationException($"Server with ID {server.Id} not found.");
        }
        
        // Check if name is already taken by another server
        var nameConflict = await _serverRepository.GetByNameAsync(server.Name, cancellationToken);
        if (nameConflict != null && nameConflict.Id != server.Id)
        {
            throw new InvalidOperationException($"A server with name '{server.Name}' already exists.");
        }
        
        // Repository will handle validation
        var updatedServer = await _serverRepository.UpdateAsync(server, cancellationToken);
        await _serverRepository.SaveChangesAsync(cancellationToken);
        
        return updatedServer;
    }

    public async Task DeleteServerAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var server = await _serverRepository.GetByIdAsync(id, cancellationToken);
        if (server == null)
        {
            throw new InvalidOperationException($"Server with ID {id} not found.");
        }
        
        await _serverRepository.DeleteAsync(server, cancellationToken);
        await _serverRepository.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Implements the demo scenario:
    /// First call: Returns servers A, B, C with initial memory values (stored in repository)
    /// Second call: Returns servers A, D, C with new memory values (A within threshold, C not within threshold)
    /// </summary>
    public async Task<IEnumerable<Server>> GetServersMemoryAsync(CancellationToken cancellationToken = default)
    {
        if (_firstCall)
        {
            // First call - return servers A, B, C with initial memory values
            _firstCall = false;
            
            return new List<Server>
            {
                new Server { Name = "A", Memory = 16.0m },  // Initial memory for A
                new Server { Name = "B", Memory = 32.0m },  // Initial memory for B
                new Server { Name = "C", Memory = 8.0m }    // Initial memory for C
            };
        }
        else
        {
            // Second call - return servers A, D, C with new memory values
            return new List<Server>
            {
                new Server { Name = "A", Memory = 18.0m },  // A's memory increased by 2GB (within threshold)
                new Server { Name = "D", Memory = 64.0m },  // New server D
                new Server { Name = "C", Memory = 24.0m }   // C's memory increased by 16GB (not within threshold)
            };
        }
    }

    /// <summary>
    /// Demonstrates the complete validation scenario as described in requirements
    /// </summary>
    public async Task DemonstrateValidationScenarioAsync(CancellationToken cancellationToken = default)
    {
        Console.WriteLine("\n--- Server Validation Scenario Demonstration ---");
        Console.WriteLine("Scenario: Validate servers based on memory threshold compared to stored values");
        
        // First call - store initial values
        Console.WriteLine("\n1. First call to getServersMemory() - storing initial servers:");
        var firstCallServers = await GetServersMemoryAsync(cancellationToken);
        
        foreach (var server in firstCallServers)
        {
            try
            {
                // Check if server already exists (by name)
                var existingServer = await GetServerByNameAsync(server.Name, cancellationToken);
                if (existingServer == null)
                {
                    await CreateServerAsync(server, cancellationToken);
                    Console.WriteLine($"   ✓ Stored server {server.Name} with {server.Memory}GB memory");
                }
                else
                {
                    existingServer.UpdateMemory(server.Memory);
                    await UpdateServerAsync(existingServer, cancellationToken);
                    Console.WriteLine($"   ✓ Updated server {server.Name} with {server.Memory}GB memory");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"   ✗ Failed to store server {server.Name}: {ex.Message}");
            }
        }
        
        Console.WriteLine("\n2. Second call to getServersMemory() - validating against stored values:");
        var secondCallServers = await GetServersMemoryAsync(cancellationToken);
        
        foreach (var server in secondCallServers)
        {
            try
            {
                var existingServer = await GetServerByNameAsync(server.Name, cancellationToken);
                
                if (existingServer == null)
                {
                    // New server (like D) - should be allowed
                    await CreateServerAsync(server, cancellationToken);
                    Console.WriteLine($"   ✓ Added new server {server.Name} with {server.Memory}GB memory");
                }
                else
                {
                    // Existing server - validate memory change
                    var memoryDifference = Math.Abs(server.Memory - existingServer.Memory);
                    var thresholdExceeded = memoryDifference > 5.0m; // 5GB threshold
                    
                    if (thresholdExceeded)
                    {
                        Console.WriteLine($"   ⚠ Server {server.Name}: Memory change from {existingServer.Memory}GB to {server.Memory}GB exceeds threshold (change: {memoryDifference}GB)");
                        // Validation should fail for this server
                    }
                    else
                    {
                        existingServer.UpdateMemory(server.Memory);
                        await UpdateServerAsync(existingServer, cancellationToken);
                        Console.WriteLine($"   ✓ Server {server.Name}: Memory updated from {existingServer.Memory}GB to {server.Memory}GB (within threshold, change: {memoryDifference}GB)");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"   ✗ Validation failed for server {server.Name}: {ex.Message}");
            }
        }
        
        Console.WriteLine("\nValidation scenario completed.");
    }

    public async Task<Server> UpdateServerMemoryAsync(Guid id, decimal newMemory, CancellationToken cancellationToken = default)
    {
        var server = await _serverRepository.GetByIdAsync(id, cancellationToken);
        if (server == null)
        {
            throw new InvalidOperationException($"Server with ID {id} not found.");
        }
        
        server.UpdateMemory(newMemory);
        
        // Repository will handle validation
        await _serverRepository.UpdateAsync(server, cancellationToken);
        await _serverRepository.SaveChangesAsync(cancellationToken);
        
        return server;
    }

    public async Task<decimal> GetAverageMemoryAsync(CancellationToken cancellationToken = default)
    {
        return await _serverRepository.GetAverageMemoryAsync(cancellationToken);
    }

    public async Task<IEnumerable<Server>> GetServersByMemoryRangeAsync(decimal minMemory, decimal maxMemory, CancellationToken cancellationToken = default)
    {
        return await _serverRepository.GetServersByMemoryRangeAsync(minMemory, maxMemory, cancellationToken);
    }
}