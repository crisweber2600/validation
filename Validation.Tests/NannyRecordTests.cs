using System;
using Validation.Domain.Entities;
using Xunit;

namespace Validation.Tests;

public class NannyRecordTests
{
    [Fact]
    public void Constructor_ValidName_CreatesRecord()
    {
        // Arrange
        const string name = "Mary Poppins";
        const string contactInfo = "mary@example.com";

        // Act
        var record = new NannyRecord(name, contactInfo);

        // Assert
        Assert.Equal(name, record.Name);
        Assert.Equal(contactInfo, record.ContactInfo);
        Assert.True(record.IsActive);
        Assert.True(record.CreatedAt <= DateTime.UtcNow);
        Assert.Null(record.LastModified);
        Assert.NotEqual(Guid.Empty, record.Id);
    }

    [Fact]
    public void Constructor_NullName_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new NannyRecord(null!));
    }

    [Fact]
    public void Constructor_EmptyName_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new NannyRecord(string.Empty));
    }

    [Fact]
    public void UpdateContactInfo_ValidInfo_UpdatesRecord()
    {
        // Arrange
        var record = new NannyRecord("Mary Poppins");
        const string newContactInfo = "mary.new@example.com";
        var originalCreatedAt = record.CreatedAt;

        // Act
        record.UpdateContactInfo(newContactInfo);

        // Assert
        Assert.Equal(newContactInfo, record.ContactInfo);
        Assert.NotNull(record.LastModified);
        Assert.True(record.LastModified > originalCreatedAt);
        Assert.Equal(originalCreatedAt, record.CreatedAt); // CreatedAt should not change
    }

    [Fact]
    public void UpdateName_ValidName_UpdatesRecord()
    {
        // Arrange
        var record = new NannyRecord("Mary Poppins");
        const string newName = "Julie Andrews";
        var originalCreatedAt = record.CreatedAt;

        // Act
        record.UpdateName(newName);

        // Assert
        Assert.Equal(newName, record.Name);
        Assert.NotNull(record.LastModified);
        Assert.True(record.LastModified > originalCreatedAt);
    }

    [Fact]
    public void UpdateName_NullName_ThrowsArgumentNullException()
    {
        // Arrange
        var record = new NannyRecord("Mary Poppins");

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => record.UpdateName(null!));
    }

    [Fact]
    public void Deactivate_ActiveRecord_DeactivatesRecord()
    {
        // Arrange
        var record = new NannyRecord("Mary Poppins");
        Assert.True(record.IsActive);

        // Act
        record.Deactivate();

        // Assert
        Assert.False(record.IsActive);
        Assert.NotNull(record.LastModified);
    }

    [Fact]
    public void Reactivate_InactiveRecord_ReactivatesRecord()
    {
        // Arrange
        var record = new NannyRecord("Mary Poppins");
        record.Deactivate();
        Assert.False(record.IsActive);

        // Act
        record.Reactivate();

        // Assert
        Assert.True(record.IsActive);
        Assert.NotNull(record.LastModified);
    }

    [Fact]
    public void Constructor_WithoutContactInfo_CreatesRecordWithNullContact()
    {
        // Arrange & Act
        var record = new NannyRecord("Mary Poppins");

        // Assert
        Assert.Equal("Mary Poppins", record.Name);
        Assert.Null(record.ContactInfo);
        Assert.True(record.IsActive);
    }

    [Fact]
    public void MultipleUpdates_LastModifiedChanges()
    {
        // Arrange
        var record = new NannyRecord("Mary Poppins");
        
        // Act
        record.UpdateName("Julie Andrews");
        var firstModified = record.LastModified;
        
        // Small delay to ensure timestamp difference
        System.Threading.Thread.Sleep(1);
        
        record.UpdateContactInfo("julie@example.com");
        var secondModified = record.LastModified;

        // Assert
        Assert.NotNull(firstModified);
        Assert.NotNull(secondModified);
        Assert.True(secondModified > firstModified);
    }
}