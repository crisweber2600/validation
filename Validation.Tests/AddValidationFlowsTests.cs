using System.Text.Json;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Validation.Domain.Entities;
using Validation.Infrastructure.DI;
using Validation.Infrastructure.Messaging;
using Validation.Domain.Validation;

namespace Validation.Tests;

public class AddValidationFlowsTests
{
    [Fact]
    public void AddValidationFlows_registers_consumers_from_config()
    {
        const string json = """
            [{
                "Type": "Validation.Domain.Entities.Item, Validation.Domain",
                "SaveValidation": true,
                "SaveCommit": true,
                "MetricProperty": "Metric",
                "ThresholdType": 1,
                "ThresholdValue": 0.2,
                "DeleteValidation": true,
                "DeleteCommit": true
            }]
            """;

        using var doc = JsonDocument.Parse(json);
        var configs = new List<ValidationFlowConfig>();
        foreach (var element in doc.RootElement.EnumerateArray())
        {
            element.TryGetProperty("ThresholdType", out var thresholdTypeElement);
            element.TryGetProperty("ThresholdValue", out var thresholdValueElement);
            
            configs.Add(new ValidationFlowConfig
            {
                Type = element.GetProperty("Type").GetString()!,
                SaveValidation = element.GetProperty("SaveValidation").GetBoolean(),
                SaveCommit = element.GetProperty("SaveCommit").GetBoolean(),
                DeleteValidation = element.GetProperty("DeleteValidation").GetBoolean(),
                DeleteCommit = element.GetProperty("DeleteCommit").GetBoolean(),
                MetricProperty = element.GetProperty("MetricProperty").GetString(),
                ThresholdType = thresholdTypeElement.ValueKind == JsonValueKind.Number 
                    ? (ThresholdType?)thresholdTypeElement.GetInt32() 
                    : null,
                ThresholdValue = thresholdValueElement.ValueKind == JsonValueKind.Number 
                    ? thresholdValueElement.GetDecimal() 
                    : null
            });
        }

        var services = new ServiceCollection();
        services.AddDbContext<TestDbContext>(o => o.UseInMemoryDatabase("validation-flows-test"));
        services.AddScoped<DbContext>(sp => sp.GetRequiredService<TestDbContext>());
        services.AddValidationFlows(configs);

        using var provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();
        
        // Verify that the consumers were registered
        Assert.NotNull(scope.ServiceProvider.GetService<SaveValidationConsumer<Item>>());
        Assert.NotNull(scope.ServiceProvider.GetService<SaveCommitConsumer<Item>>());
        Assert.NotNull(scope.ServiceProvider.GetService<DeleteValidationConsumer<Item>>());
        Assert.NotNull(scope.ServiceProvider.GetService<DeleteCommitConsumer<Item>>());
        
        // Verify that validation plan provider was configured
        var planProvider = scope.ServiceProvider.GetRequiredService<IValidationPlanProvider>();
        Assert.NotNull(planProvider);
    }

    [Fact]
    public void AddValidationFlows_supports_configuration_without_validation_plan()
    {
        var configs = new List<ValidationFlowConfig>
        {
            new ValidationFlowConfig
            {
                Type = "Validation.Domain.Entities.Item, Validation.Domain",
                SaveValidation = true,
                SaveCommit = false,
                // No MetricProperty, ThresholdType, or ThresholdValue
            }
        };

        var services = new ServiceCollection();
        services.AddDbContext<TestDbContext>(o => o.UseInMemoryDatabase("validation-flows-no-plan"));
        services.AddScoped<DbContext>(sp => sp.GetRequiredService<TestDbContext>());
        services.AddValidationFlows(configs);

        using var provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();
        
        // Verify that only SaveValidationConsumer was registered
        Assert.NotNull(scope.ServiceProvider.GetService<SaveValidationConsumer<Item>>());
        Assert.Null(scope.ServiceProvider.GetService<SaveCommitConsumer<Item>>());
        
        // Verify that validation plan provider was still configured
        var planProvider = scope.ServiceProvider.GetRequiredService<IValidationPlanProvider>();
        Assert.NotNull(planProvider);
    }

    [Fact]
    public void AddValidationFlows_registers_manual_rules_from_config()
    {
        const string json = """
            [{
                "Type": "Validation.Domain.Entities.Item, Validation.Domain",
                "SaveValidation": false,
                "SaveCommit": false,
                "DeleteValidation": false,
                "DeleteCommit": false,
                "ManualRules": ["Metric>50"]
            }]
            """;

        using var doc = JsonDocument.Parse(json);
        var configs = new List<ValidationFlowConfig>();
        foreach (var element in doc.RootElement.EnumerateArray())
        {
            configs.Add(new ValidationFlowConfig
            {
                Type = element.GetProperty("Type").GetString()!,
                SaveValidation = element.GetProperty("SaveValidation").GetBoolean(),
                SaveCommit = element.GetProperty("SaveCommit").GetBoolean(),
                DeleteValidation = element.GetProperty("DeleteValidation").GetBoolean(),
                DeleteCommit = element.GetProperty("DeleteCommit").GetBoolean(),
                ManualRules = element.GetProperty("ManualRules").EnumerateArray().Select(e => e.GetString()!).ToList()
            });
        }

        var services = new ServiceCollection();
        services.AddDbContext<TestDbContext>(o => o.UseInMemoryDatabase("manual-rules"));
        services.AddScoped<DbContext>(sp => sp.GetRequiredService<TestDbContext>());
        services.AddValidationFlows(configs);

        using var provider = services.BuildServiceProvider();
        var validator = provider.GetRequiredService<IManualValidatorService>();

        Assert.True(validator.Validate(new Item(60)));
        Assert.False(validator.Validate(new Item(40)));
    }
}