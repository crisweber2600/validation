using Validation.Domain.Providers;
using Validation.Infrastructure.Repositories;
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
    private readonly ISaveAuditRepository _auditRepository;
    private readonly IApplicationNameProvider _applicationNameProvider;
    private bool _firstCall = true;
    private const decimal MemoryThreshold = 5.0m; // 5GB threshold for validation

    public ServerService(IServerRepository serverRepository, 
                        ISaveAuditRepository auditRepository,
                        IApplicationNameProvider applicationNameProvider)
    {
        _serverRepository = serverRepository ?? throw new ArgumentNullException(nameof(serverRepository));
        _auditRepository = auditRepository ?? throw new ArgumentNullException(nameof(auditRepository));
        _applicationNameProvider = applicationNameProvider ?? throw new ArgumentNullException(nameof(applicationNameProvider));
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
        
        // Get last audited memory value for this server
        var lastAudit = await _auditRepository.GetLastAuditAsync(server.Name, "Memory", cancellationToken);
        var previousMemory = lastAudit?.PropertyValue ?? 0m;
        
        // Validate memory change (for new servers, any positive value is valid)
        bool isValid = server.Memory > 0 && (lastAudit == null || Math.Abs(server.Memory - previousMemory) <= MemoryThreshold);
        
        // Repository will handle standard validation
        var addedServer = await _serverRepository.AddAsync(server, cancellationToken);
        await _serverRepository.SaveChangesAsync(cancellationToken);
        
        // Record the memory audit
        await _auditRepository.AddOrUpdateAuditAsync(
            server.Name,
            nameof(Server),
            "Memory",
            server.Memory,
            isValid,
            _applicationNameProvider.GetApplicationName(),
            "Create",
            null,
            cancellationToken);
        
        if (!isValid)
        {
            Console.WriteLine($"   ⚠ Server {server.Name}: Memory validation failed - change from {previousMemory}GB to {server.Memory}GB exceeds threshold");
        }
        
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
        
        // Get last audited memory value for this server
        var lastAudit = await _auditRepository.GetLastAuditAsync(server.Name, "Memory", cancellationToken);
        var previousMemory = lastAudit?.PropertyValue ?? existingServer.Memory;
        
        // Validate memory change
        bool isValid = server.Memory > 0 && Math.Abs(server.Memory - previousMemory) <= MemoryThreshold;
        
        // Repository will handle standard validation
        var updatedServer = await _serverRepository.UpdateAsync(server, cancellationToken);
        await _serverRepository.SaveChangesAsync(cancellationToken);
        
        // Record the memory audit
        await _auditRepository.AddOrUpdateAuditAsync(
            server.Name,
            nameof(Server),
            "Memory",
            server.Memory,
            isValid,
            _applicationNameProvider.GetApplicationName(),
            "Update",
            null,
            cancellationToken);
        
        if (!isValid)
        {
            Console.WriteLine($"   ⚠ Server {server.Name}: Memory validation failed - change from {previousMemory}GB to {server.Memory}GB exceeds threshold (change: {Math.Abs(server.Memory - previousMemory)}GB)");
        }
        
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
        Console.WriteLine("Scenario: Property-aware auditing with memory threshold validation");
        
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
        
        Console.WriteLine("\n2. Second call to getServersMemory() - validating against audited values:");
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
                    Console.WriteLine($"   ✓ Added new server {server.Name} with {server.Memory}GB memory (valid by default)");
                }
                else
                {
                    // Existing server - validate memory change using property-aware auditing
                    var lastAudit = await _auditRepository.GetLastAuditAsync(server.Name, "Memory", cancellationToken);
                    var previousMemory = lastAudit?.PropertyValue ?? existingServer.Memory;
                    var memoryDifference = Math.Abs(server.Memory - previousMemory);
                    var withinThreshold = memoryDifference <= MemoryThreshold;
                    
                    existingServer.UpdateMemory(server.Memory);
                    await UpdateServerAsync(existingServer, cancellationToken);
                    
                    if (withinThreshold)
                    {
                        Console.WriteLine($"   ✓ Server {server.Name}: Memory updated from {previousMemory}GB to {server.Memory}GB (within threshold, change: {memoryDifference}GB)");
                    }
                    else
                    {
                        Console.WriteLine($"   ⚠ Server {server.Name}: Memory change from {previousMemory}GB to {server.Memory}GB exceeds threshold (change: {memoryDifference}GB) - marked as invalid in audit");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"   ✗ Validation failed for server {server.Name}: {ex.Message}");
            }
        }
        
        // Display audit summary
        Console.WriteLine("\n3. Property-aware audit summary:");
        var allServers = await GetAllServersAsync(cancellationToken);
        foreach (var server in allServers)
        {
            var audit = await _auditRepository.GetLastAuditAsync(server.Name, "Memory", cancellationToken);
            if (audit != null)
            {
                var status = audit.IsValid ? "✓ Valid" : "✗ Invalid";
                Console.WriteLine($"   Server {server.Name}: Memory={audit.PropertyValue}GB, Status={status}, Operation={audit.OperationType}");
            }
        }
        
        Console.WriteLine("\nProperty-aware validation scenario completed.");
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