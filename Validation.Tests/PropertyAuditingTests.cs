using System;
using System.Threading;
using System.Threading.Tasks;
using Validation.Infrastructure;
using Validation.Tests;
using Xunit;

namespace PropertyAuditingTests
{
    /// <summary>
    /// Simple test to validate the property-aware auditing functionality
    /// </summary>
    public class PropertyAuditingTests
    {
        [Fact]
        public async Task PropertyAwareAuditing_ShouldTrackEntityPropertyChanges()
        {
            // Arrange
            var repository = new InMemorySaveAuditRepository();
            var entityId = "Server-A";
            var propertyName = "Memory";
            
            // Act - First audit (create)
            await repository.AddOrUpdateAuditAsync(
                entityId, "Server", propertyName, 16.0m, true, 
                "TestApp", "Create", "test-correlation");
            
            // Get the initial audit
            var firstAudit = await repository.GetLastAuditAsync(entityId, propertyName);
            
            // Act - Second audit (update) - within threshold
            await repository.AddOrUpdateAuditAsync(
                entityId, "Server", propertyName, 18.0m, true, 
                "TestApp", "Update", "test-correlation-2");
            
            // Get the updated audit
            var secondAudit = await repository.GetLastAuditAsync(entityId, propertyName);
            
            // Act - Third audit (update) - exceeds threshold
            await repository.AddOrUpdateAuditAsync(
                entityId, "Server", propertyName, 24.0m, false, 
                "TestApp", "Update", "test-correlation-3");
            
            // Get the final audit
            var finalAudit = await repository.GetLastAuditAsync(entityId, propertyName);
            
            // Assert - The audit should be updated in place (same record, updated values)
            Assert.NotNull(firstAudit);
            Assert.NotNull(secondAudit);
            Assert.NotNull(finalAudit);
            
            // All audits should reference the same record (updated in place)
            Assert.Equal(firstAudit.Id, secondAudit.Id);
            Assert.Equal(secondAudit.Id, finalAudit.Id);
            
            // Check entity and property details
            Assert.Equal(entityId, finalAudit.EntityId);
            Assert.Equal(propertyName, finalAudit.PropertyName);
            Assert.Equal("Server", finalAudit.EntityType);
            
            // Final audit should have the latest values
            Assert.Equal(24.0m, finalAudit.PropertyValue);
            Assert.False(finalAudit.IsValid);
            Assert.Equal("Update", finalAudit.OperationType);
            Assert.Equal("test-correlation-3", finalAudit.CorrelationId);
            
            // Verify total number of audits (should only have one per entity/property)
            Assert.Single(repository.Audits);
        }
        
        [Fact]
        public async Task PropertyAwareAuditing_ShouldHandleMultipleProperties()
        {
            // Arrange
            var repository = new InMemorySaveAuditRepository();
            var entityId = "Server-B";
            
            // Act - Audit different properties
            await repository.AddOrUpdateAuditAsync(
                entityId, "Server", "Memory", 32.0m, true, "TestApp", "Create");
            
            await repository.AddOrUpdateAuditAsync(
                entityId, "Server", "CPU", 4.0m, true, "TestApp", "Create");
            
            // Get audits for different properties
            var memoryAudit = await repository.GetLastAuditAsync(entityId, "Memory");
            var cpuAudit = await repository.GetLastAuditAsync(entityId, "CPU");
            
            // Assert
            Assert.NotNull(memoryAudit);
            Assert.NotNull(cpuAudit);
            Assert.NotEqual(memoryAudit.Id, cpuAudit.Id);
            Assert.Equal("Memory", memoryAudit.PropertyName);
            Assert.Equal("CPU", cpuAudit.PropertyName);
            Assert.Equal(32.0m, memoryAudit.PropertyValue);
            Assert.Equal(4.0m, cpuAudit.PropertyValue);
            
            // Should have two separate audit records
            Assert.Equal(2, repository.Audits.Count);
        }
    }
}