using System.Text.Json;
using System.Text.Json.Serialization;
using System.Collections.Generic;
using Validation.Domain.Validation;
using MassTransit;
using Validation.Domain.Entities;
using Validation.Infrastructure.DI;
using Microsoft.Extensions.DependencyInjection;
using MassTransit.Testing;
using Validation.Infrastructure.Messaging;

namespace Validation.Tests;

public class AddValidationFlowsTests
{
    // Sample JSON config
    /*
     {
       "Type":"ExampleData.YourEntity, ExampleData",
       "SaveValidation":true,
       "SaveCommit":true,
       "MetricProperty":"Id",
       "ThresholdType":"PercentChange",
       "ThresholdValue":0.2
     }
    */

    [Fact]
    public void Consumers_are_registered_from_json_config()
    {
        var json = "{\n" +
                   "  \"Type\":\"Validation.Domain.Entities.Item, Validation.Domain\",\n" +
                   "  \"SaveValidation\":true,\n" +
                   "  \"SaveCommit\":true,\n" +
                   "  \"MetricProperty\":\"Id\",\n" +
                   "  \"ThresholdType\":1,\n" +
                   "  \"ThresholdValue\":0.2\n" +
                   "}";
        using var doc = JsonDocument.Parse("[" + json + "]");
        var list = new List<ValidationFlowConfig>();
        foreach (var el in doc.RootElement.EnumerateArray())
        {
            var cfg = new ValidationFlowConfig
            {
                Type = el.GetProperty("Type").GetString()!,
                SaveValidation = el.GetProperty("SaveValidation").GetBoolean(),
                SaveCommit = el.GetProperty("SaveCommit").GetBoolean(),
                MetricProperty = el.GetProperty("MetricProperty").GetString()!,
                ThresholdType = (ThresholdType)el.GetProperty("ThresholdType").GetInt32(),
                ThresholdValue = el.GetProperty("ThresholdValue").GetDecimal()
            };
            list.Add(cfg);
        }
        var config = list.ToArray();

        var services = new ServiceCollection();
        services.AddValidationFlows(config);

        var provider = services.BuildServiceProvider(true);
        var context = provider.GetRequiredService<IBusRegistrationContext>();

        Assert.NotNull(context);
    }
}
