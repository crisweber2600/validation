using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Validation.Domain.Entities;
using Validation.Infrastructure.DI;
using Validation.Infrastructure.Messaging;

namespace Validation.Tests;

public class AddValidationFlowsTests
{
    [Fact]
    public void Configuration_registers_consumers()
    {
        var json = "[\n  {\n    \"Type\":\"Validation.Domain.Entities.Item, Validation.Domain\",\n    \"SaveValidation\":true,\n    \"SaveCommit\":true,\n    \"MetricProperty\":\"Metric\",\n    \"ThresholdType\":\"PercentChange\",\n    \"ThresholdValue\":0.2\n  }\n]";
        var opts = new JsonSerializerOptions();
        opts.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
        var configs = JsonSerializer.Deserialize<List<ValidationFlowConfig>>(json, opts)!;

        var services = new ServiceCollection();
        services.AddValidationFlows(configs);

        var hasValidation = services.Any(d => d.ServiceType == typeof(SaveValidationConsumer<Item>));
        var hasCommit = services.Any(d => d.ServiceType == typeof(SaveCommitConsumer<Item>));

        Assert.True(hasValidation);
        Assert.True(hasCommit);
    }
}
