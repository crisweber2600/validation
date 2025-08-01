using System;
using Xunit;
using Validation.Domain.Providers;

namespace Validation.Tests;

public class ConfigurableEntityIdProviderTests
{
    private class TestEntity
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = string.Empty;
        public int Value { get; set; }
    }

    [Fact]
    public void RegisterSelector_WithStringSelector_CreatesDeterministicGuid()
    {
        // Arrange
        var provider = new ConfigurableEntityIdProvider();
        provider.RegisterSelector<TestEntity>(e => e.Name);

        var entity1 = new TestEntity { Name = "TestServer" };
        var entity2 = new TestEntity { Name = "TestServer" };

        // Act
        var id1 = provider.GetEntityId(entity1);
        var id2 = provider.GetEntityId(entity2);

        // Assert
        Assert.Equal(id1, id2); // Same name should produce same GUID
        Assert.NotEqual(Guid.Empty, id1);
    }

    [Fact]
    public void RegisterSelector_WithDifferentNames_CreatesDifferentGuids()
    {
        // Arrange
        var provider = new ConfigurableEntityIdProvider();
        provider.RegisterSelector<TestEntity>(e => e.Name);

        var entity1 = new TestEntity { Name = "ServerA" };
        var entity2 = new TestEntity { Name = "ServerB" };

        // Act
        var id1 = provider.GetEntityId(entity1);
        var id2 = provider.GetEntityId(entity2);

        // Assert
        Assert.NotEqual(id1, id2); // Different names should produce different GUIDs
    }

    [Fact]
    public void GetEntityId_NoSelectorRegistered_FallsBackToIdProperty()
    {
        // Arrange
        var provider = new ConfigurableEntityIdProvider();
        var entityId = Guid.NewGuid();
        var entity = new TestEntity { Id = entityId, Name = "TestServer" };

        // Act
        var result = provider.GetEntityId(entity);

        // Assert
        Assert.Equal(entityId, result);
    }

    [Fact]
    public void GetEntityId_NoSelectorAndNoId_GeneratesNewGuid()
    {
        // Arrange
        var provider = new ConfigurableEntityIdProvider();
        var entity = new { Name = "TestServer", Value = 42 }; // Anonymous type without Id

        // Act
        var result = provider.GetEntityId(entity);

        // Assert
        Assert.NotEqual(Guid.Empty, result);
    }

    [Fact]
    public void CanHandle_WithRegisteredSelector_ReturnsTrue()
    {
        // Arrange
        var provider = new ConfigurableEntityIdProvider();
        provider.RegisterSelector<TestEntity>(e => e.Name);

        // Act
        var result = provider.CanHandle<TestEntity>();

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void CanHandle_WithIdProperty_ReturnsTrue()
    {
        // Arrange
        var provider = new ConfigurableEntityIdProvider();

        // Act
        var result = provider.CanHandle<TestEntity>();

        // Assert
        Assert.True(result); // Should return true because TestEntity has Id property
    }
}

public class ReflectionBasedEntityIdProviderTests
{
    private class TestEntity
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
    }

    [Fact]
    public void GetEntityId_WithNameProperty_UsesPriorityOrder()
    {
        // Arrange
        var provider = new ReflectionBasedEntityIdProvider("Name", "Code");
        
        var entity1 = new TestEntity { Name = "TestName", Code = "TestCode" };
        var entity2 = new TestEntity { Name = "TestName", Code = "DifferentCode" };

        // Act
        var id1 = provider.GetEntityId(entity1);
        var id2 = provider.GetEntityId(entity2);

        // Assert
        Assert.Equal(id1, id2); // Should use Name (higher priority) and ignore Code
    }

    [Fact]
    public void GetEntityId_WithoutPriorityProperty_FallsBackToId()
    {
        // Arrange
        var provider = new ReflectionBasedEntityIdProvider("NonExistent");
        var entityId = Guid.NewGuid();
        var entity = new TestEntity { Id = entityId, Name = "TestName" };

        // Act
        var result = provider.GetEntityId(entity);

        // Assert
        Assert.Equal(entityId, result);
    }

    [Fact]
    public void CanHandle_WithPriorityProperty_ReturnsTrue()
    {
        // Arrange
        var provider = new ReflectionBasedEntityIdProvider("Name");

        // Act
        var result = provider.CanHandle<TestEntity>();

        // Assert
        Assert.True(result);
    }
}