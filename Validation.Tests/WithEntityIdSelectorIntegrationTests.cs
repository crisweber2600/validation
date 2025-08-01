using System;
using System.Threading.Tasks;
using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Xunit;
using Validation.Domain.Events;
using Validation.Domain.Providers;
using Validation.Infrastructure.DI;
using Validation.Infrastructure.Repositories;

namespace Validation.Tests;

// Test entity to simulate Server
public class TestServer
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public decimal Memory { get; set; }
}

public class WithEntityIdSelectorIntegrationTests
{
    [Fact]
    public async Task WithEntityIdSelector_ForServer_GeneratesDeterministicEntityIds()
    {
        // Arrange
        var services = new ServiceCollection();
        
        // Configure with WithEntityIdSelector for TestServer
        services.WithEntityIdSelector<TestServer>(s => s.Name);
        
        // Get the provider and verify it works correctly
        var serviceProvider = services.BuildServiceProvider();
        var entityIdProvider = serviceProvider.GetRequiredService<IEntityIdProvider>();
        
        // Act
        var serverA1 = new TestServer { Name = "ServerA", Memory = 16 };
        var serverA2 = new TestServer { Name = "ServerA", Memory = 18 }; // Same name, different memory
        var serverB = new TestServer { Name = "ServerB", Memory = 32 };
        
        var idA1 = entityIdProvider.GetEntityId(serverA1);
        var idA2 = entityIdProvider.GetEntityId(serverA2);
        var idB = entityIdProvider.GetEntityId(serverB);
        
        // Assert
        Assert.Equal(idA1, idA2); // Same name should produce same ID
        Assert.NotEqual(idA1, idB); // Different names should produce different IDs
        Assert.NotEqual(Guid.Empty, idA1);
        Assert.NotEqual(Guid.Empty, idB);
    }
    
    [Fact]
    public void ConfigurableEntityIdProvider_WithServerSelector_CanHandleServerType()
    {
        // Arrange
        var services = new ServiceCollection();
        services.WithEntityIdSelector<TestServer>(s => s.Name);
        var serviceProvider = services.BuildServiceProvider();
        var provider = serviceProvider.GetRequiredService<IEntityIdProvider>();
        
        // Act & Assert
        Assert.True(provider.CanHandle<TestServer>());
        Assert.IsType<ConfigurableEntityIdProvider>(provider);
    }
    
    [Fact]
    public async Task EventPublishingRepository_WithConfiguredProvider_UsesEntityIdFromProvider()
    {
        // Arrange
        var services = new ServiceCollection();
        services.WithEntityIdSelector<TestServer>(s => s.Name);
        
        // Add required dependencies for EventPublishingRepository
        services.AddSingleton<IApplicationNameProvider>(sp => new StaticApplicationNameProvider("TestApp"));
        services.AddScoped<EventPublishingRepository<TestServer>>(); // Register the repository
        services.AddMassTransitTestHarness(x =>
        {
            x.UsingInMemory((context, cfg) => cfg.ConfigureEndpoints(context));
        });
        
        var serviceProvider = services.BuildServiceProvider();
        await using var scope = serviceProvider.CreateAsyncScope();
        
        var harness = scope.ServiceProvider.GetRequiredService<ITestHarness>();
        await harness.Start();
        
        try
        {
            var repository = scope.ServiceProvider.GetRequiredService<EventPublishingRepository<TestServer>>();
            var entityIdProvider = scope.ServiceProvider.GetRequiredService<IEntityIdProvider>();
            
            // Act
            var server = new TestServer { Name = "ServerA", Memory = 16 };
            var expectedEntityId = entityIdProvider.GetEntityId(server);
            
            await repository.SaveAsync(server);
            
            // Assert
            // Check that SaveRequested<TestServer> event was published
            Assert.True(await harness.Published.Any<SaveRequested<TestServer>>());
            
            // Get the published event and verify the entity matches
            var publishedEvent = await harness.Published.SelectAsync<SaveRequested<TestServer>>().FirstOrDefault();
            Assert.NotNull(publishedEvent);
            Assert.Equal("ServerA", publishedEvent.Context.Message.Entity.Name);
            Assert.Equal(16m, publishedEvent.Context.Message.Entity.Memory);
        }
        finally
        {
            await harness.Stop();
        }
    }
}