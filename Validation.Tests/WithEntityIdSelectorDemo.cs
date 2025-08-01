using System;
using Validation.Domain.Providers;

// Simple demonstration of the WithEntityIdSelector functionality
Console.WriteLine("WithEntityIdSelector Pattern Demonstration");
Console.WriteLine("==========================================");

// Simulate Server entities like in the requirement
var servers = new[]
{
    new { Name = "ServerA", Memory = 8 },
    new { Name = "ServerB", Memory = 16 },
    new { Name = "ServerC", Memory = 32 },
    new { Name = "ServerA", Memory = 12 }, // Same name as first, should get same ID
};

// Create and configure the provider
var provider = new ConfigurableEntityIdProvider();
provider.RegisterSelector<dynamic>(s => s.Name);

Console.WriteLine("Server Entity IDs (using Name property):");
Console.WriteLine("-----------------------------------------");

foreach (var server in servers)
{
    var entityId = provider.GetEntityId(server);
    Console.WriteLine($"Server: {server.Name,-10} Memory: {server.Memory,2}GB => EntityId: {entityId}");
}

Console.WriteLine();
Console.WriteLine("Notice that servers with the same name get the same deterministic GUID!");
Console.WriteLine("This enables SaveAudit records to be grouped by Server.Name instead of random GUIDs.");

// Demonstrate the ReflectionBasedEntityIdProvider
Console.WriteLine();
Console.WriteLine("ReflectionBasedEntityIdProvider with priority [\"Name\", \"Code\"]:");
Console.WriteLine("------------------------------------------------------------------");

var reflectionProvider = new ReflectionBasedEntityIdProvider("Name", "Code");
var testEntities = new[]
{
    new { Name = "Entity1", Code = "E001", Id = Guid.NewGuid() },
    new { Name = "", Code = "E002", Id = Guid.NewGuid() }, // Empty name, should use Code
    new { Title = "Entity3", Id = Guid.NewGuid() }, // No Name or Code, should use Id
};

foreach (var entity in testEntities)
{
    var entityId = reflectionProvider.GetEntityId(entity);
    Console.WriteLine($"Entity => EntityId: {entityId}");
}