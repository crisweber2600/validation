using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.EntityFrameworkCore;
using Validation.Domain.Entities;
using Validation.Domain.Validation;
using Validation.Infrastructure.Setup;
using Validation.Infrastructure.DI;
using Validation.Infrastructure;
using Xunit;

namespace Validation.Tests;

public class UnifiedValidationSystemTests
{
    [Fact]
    public void SetupValidationBuilder_BasicConfiguration_RegistersServices()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddSetupValidation()
            .UseEntityFramework<TestDbContext>()
            .AddValidationFlow<Item>(flow => flow
                .EnableSaveValidation()
                .EnableSaveCommit()
                .EnableDeleteValidation()
                .EnableSoftDelete()
                .WithThreshold(x => x.Metric, ThresholdType.GreaterThan, 100))
            .AddRule<Item>(item => item.Metric > 0)
            .ConfigureMetrics(metrics => metrics
                .WithProcessingInterval(TimeSpan.FromMinutes(1))
                .EnableDetailedMetrics())
            .ConfigureReliability(reliability => reliability
                .WithMaxRetries(3)
                .WithRetryDelay(TimeSpan.FromSeconds(1)))
            .Build();

        // Assert
        var provider = services.BuildServiceProvider();
        Assert.NotNull(provider.GetService<IValidationPlanProvider>());
        Assert.NotNull(provider.GetService<IManualValidatorService>());
        Assert.NotNull(provider.GetService<IEnhancedManualValidatorService>());
        Assert.NotNull(provider.GetService<DbContext>());
    }

    [Fact]
    public void AddValidation_SimpleConfiguration_RegistersBasicServices()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddValidation(setup => setup
            .AddValidationFlow<Item>(flow => flow.EnableSaveValidation())
            .AddRule<Item>(item => item.Metric > 0));

        // Assert
        var provider = services.BuildServiceProvider();
        Assert.NotNull(provider.GetService<IValidationPlanProvider>());
        Assert.NotNull(provider.GetService<IManualValidatorService>());
    }

    [Fact]
    public async Task EnhancedManualValidatorService_WithNamedRules_ValidatesCorrectly()
    {
        // Arrange
        var validator = new EnhancedManualValidatorService(NullLogger<EnhancedManualValidatorService>.Instance);
        validator.AddRule<Item>("PositiveValue", item => item.Metric > 0);
        validator.AddRule<Item>("MaxValue", item => item.Metric <= 1000);

        var validItem = new Item(500);
        var invalidItem = new Item(-1);

        // Act
        var validResult = validator.ValidateWithDetails(validItem);
        var invalidResult = validator.ValidateWithDetails(invalidItem);

        // Assert
        Assert.True(validResult.IsValid);
        Assert.Empty(validResult.FailedRules);

        Assert.False(invalidResult.IsValid);
        Assert.Contains("PositiveValue", invalidResult.FailedRules);
        Assert.DoesNotContain("MaxValue", invalidResult.FailedRules);
    }

    [Fact]
    public void ValidationFlowConfig_ExtendedProperties_ConfiguresCorrectly()
    {
        // Arrange & Act
        var config = new ValidationFlowConfig
        {
            Type = typeof(Item).AssemblyQualifiedName!,
            SaveValidation = true,
            SaveCommit = true,
            DeleteValidation = true,
            DeleteCommit = true,
            SoftDeleteSupport = true,
            EnableAuditing = true,
            EnableMetrics = true,
            ValidationTimeout = TimeSpan.FromMinutes(5),
            MaxRetryAttempts = 3
        };

        // Assert
        Assert.True(config.SaveValidation);
        Assert.True(config.DeleteValidation);
        Assert.True(config.SoftDeleteSupport);
        Assert.True(config.EnableAuditing);
        Assert.Equal(TimeSpan.FromMinutes(5), config.ValidationTimeout);
        Assert.Equal(3, config.MaxRetryAttempts);
    }

    [Fact]
    public void ValidationFlowBuilder_FluentConfiguration_BuildsCorrectConfig()
    {
        // Arrange
        var builder = new ValidationFlowBuilder<Item>();

        // Act
        var config = builder
            .EnableSaveValidation()
            .EnableDeleteValidation()
            .EnableSoftDelete()
            .WithValidationTimeout(TimeSpan.FromMinutes(2))
            .WithMaxRetryAttempts(5)
            .EnableAuditing()
            .DisableMetrics()
            .WithThreshold(x => x.Metric, ThresholdType.LessThan, 1000)
            .Build();

        // Assert
        Assert.True(config.SaveValidation);
        Assert.True(config.DeleteValidation);
        Assert.True(config.SoftDeleteSupport);
        Assert.True(config.EnableAuditing);
        Assert.False(config.EnableMetrics);
        Assert.Equal(TimeSpan.FromMinutes(2), config.ValidationTimeout);
        Assert.Equal(5, config.MaxRetryAttempts);
        Assert.Equal("Metric", config.MetricProperty);
        Assert.Equal(ThresholdType.LessThan, config.ThresholdType);
        Assert.Equal(1000, config.ThresholdValue);
    }

    [Fact]
    public void UnifiedValidationEvents_ImplementInterfaces_CorrectlyStructured()
    {
        // Arrange & Act
        var deleteEvent = new Validation.Domain.Events.DeleteValidationCompleted(
            Guid.NewGuid(), "Item", true, Guid.NewGuid(), "Test audit");
        
        var saveEvent = new Validation.Domain.Events.SaveValidationCompleted(
            Guid.NewGuid(), "Item", true, new { Value = 100 });

        var failureEvent = new Validation.Domain.Events.ValidationOperationFailed(
            Guid.NewGuid(), "Item", "Save", "Test error", null, 2);

        // Assert
        Assert.IsAssignableFrom<Validation.Domain.Events.IAuditableEvent>(deleteEvent);
        Assert.IsAssignableFrom<Validation.Domain.Events.IValidationEvent>(deleteEvent);
        Assert.IsAssignableFrom<Validation.Domain.Events.IAuditableEvent>(saveEvent);
        Assert.IsAssignableFrom<Validation.Domain.Events.IRetryableEvent>(failureEvent);
        Assert.Equal(2, failureEvent.AttemptNumber);
    }
}

public static class ValidationFlowBuilderExtensions
{
    public static ValidationFlowBuilder<T> DisableMetrics<T>(this ValidationFlowBuilder<T> builder)
    {
        return builder.EnableMetrics(false);
    }
}